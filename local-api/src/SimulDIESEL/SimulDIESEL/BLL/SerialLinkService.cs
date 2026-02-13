using System;
using System.IO.Ports;
using System.Text;
using SimulDIESEL.DAL;

namespace SimulDIESEL.BLL
{
    /// <summary>
    /// BLL mínima para conexão serial e handshake de link.
    /// UI fala com esta classe. Ela usa a DAL (SerialTransport).
    /// </summary>
    public sealed class SerialLinkService : IDisposable
    {
        // =========================================================
        // Infra / Comum
        // =========================================================
        private readonly SerialTransport _transport;
        private bool _disposed;

        // =========================================================
        // SD-GW-LINK Engine + Health
        // =========================================================
        private SdGwLinkEngine _engine;
        private SdGwHealthService _health;

        public enum LinkState
        {
            Disconnected,
            SerialConnected,
            Draining,
            BannerSent,
            Linked,
            LinkFailed
        }

        public event Action<LinkState> LinkStateChanged;

        public LinkState State { get; private set; } = LinkState.Disconnected;

        public bool IsLinked => State == LinkState.Linked;

        // "Connected" = transporte aberto
        public bool IsConnected => _transport.IsOpen;

        public bool IsSerialOpen => _transport.IsOpen;

        public event Action<bool> ConnectionChanged;
        public event Action<string[]> Error;
        public event Action<byte[]> BytesReceived; // opcional (debug)

        private void SetState(LinkState newState)
        {
            if (State == newState) return;

            State = newState;
            LinkStateChanged?.Invoke(State);

            Console.WriteLine($" SerialLinkService. LinkStateChanged => {State}");
        }

        public SerialLinkService()
        {
            _transport = new SerialTransport();

            // =======================
            // Subscrições do transporte (defensivo)
            // =======================
            _transport.ConnectionChanged -= OnTransportConnectionChanged;
            _transport.Error -= OnTransportError;
            _transport.BytesReceived -= OnTransportBytesReceived;

            _transport.ConnectionChanged += OnTransportConnectionChanged;
            _transport.Error += OnTransportError;
            _transport.BytesReceived += OnTransportBytesReceived;

            // =======================
            // SD-GW-LINK Engine
            // =======================
            var cfg = new SdGwLinkEngine.Config
            {
                CmdAck = 0xF1,
                CmdErr = 0xF2,
                FlagAckReq = 0x01,
                FlagIsEvt = 0x02,
                MaxRawFrameLen = 250
            };

            _engine = new SdGwLinkEngine(cfg, WriteRaw);

            // =======================
            // Health / Ping Service
            // =======================
            var healthCfg = new SdGwHealthService.Config
            {
                PingCmd = 0x55,
                PingIntervalMs = 1000,
                PingTimeoutMs = 150,
                PingRetries = 2
            };

            _health = new SdGwHealthService(_engine, healthCfg);

            // quando health muda, isso faz parte do Link (defensivo)
            _health.LinkHealthChanged -= Health_LinkHealthChanged;
            _health.LinkHealthChanged += Health_LinkHealthChanged;

            // Só ativa ping quando estiver Linked (defensivo)
            LinkStateChanged -= OnLinkStateChanged_ForHealth;
            LinkStateChanged += OnLinkStateChanged_ForHealth;
        }

        private void OnLinkStateChanged_ForHealth(LinkState st)
        {
            Console.WriteLine("Health enable? state=" + st);
            if (_health == null) return;
            _health.SetEnabled(st == LinkState.Linked);
        }

        private void Health_LinkHealthChanged(bool alive)
        {
            // Pode vir de thread de Timer
            if (_disposed) return;

            // Se não estiver LINKED, health não deve mandar no estado do link
            if (State != LinkState.Linked)
                return;

            // Se a serial caiu, o próprio ConnectionChanged já derruba tudo
            if (!_transport.IsOpen)
                return;

            if (!alive)
            {
                Console.WriteLine(" SerialLinkService. Health DEAD => Link caiu. Reiniciando tentativa de link.");

                lock (_linkSync)
                {
                    // derruba link (mas NÃO desconecta serial)
                    SetState(LinkState.LinkFailed);

                    // reseta tentativa para re-handshake
                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();

                    // tenta imediatamente
                    _nextAttemptAtUtc = DateTime.UtcNow;

                    // garante que o loop de handshake está rodando
                    StartLinkLoop_NoLock();
                }
            }
        }

        public static string[] ListarPortas()
        {
            return SerialTransport.ListPorts();
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopLinkTimer();

            // Desinscrições (defensivo)
            _transport.ConnectionChanged -= OnTransportConnectionChanged;
            _transport.Error -= OnTransportError;
            _transport.BytesReceived -= OnTransportBytesReceived;

            if (_health != null)
            {
                _health.LinkHealthChanged -= Health_LinkHealthChanged;
            }

            LinkStateChanged -= OnLinkStateChanged_ForHealth;

            if (_health != null) _health.Dispose();
            _health = null;
            _engine = null;

            _transport.Dispose();
        }

        // =========================================================
        // Seção: Conexão Serial (abrir/fechar + eventos)
        // =========================================================
        #region Serial Connection

        public bool Connect(
            string portName,
            int baudRate,
            bool dtrEnable = false,
            bool rtsEnable = false,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SerialLinkService));

            return _transport.Connect(
                portName: portName,
                baudRate: baudRate,
                parity: parity,
                dataBits: dataBits,
                stopBits: stopBits,
                handshake: handshake,
                dtrEnable: dtrEnable,
                rtsEnable: rtsEnable
            );
        }

        public void Disconnect()
        {
            if (_disposed) return;
            _transport.Disconnect();
        }

        /// <summary>
        /// Método público para o Engine escrever bytes no transporte (stream).
        /// </summary>
        public bool WriteRaw(byte[] data)
        {
            if (_disposed) return false;
            return _transport.Write(data);
        }

        private void OnTransportConnectionChanged(bool connected)
        {
            ConnectionChanged?.Invoke(connected);

            Console.WriteLine($" SerialLinkService. ConnectionChanged Invoked. Connected={connected}");

            lock (_linkSync)
            {
                if (!connected)
                {
                    // Transporte caiu => link cai junto, para tentativas
                    _attemptActive = false;
                    _rxBuffer.Clear();
                    StopLinkTimer();
                    SetState(LinkState.Disconnected);
                    return;
                }

                // Transporte voltou (IsOpen == true) => inicia tentativas automáticas de link
                SetState(LinkState.SerialConnected);

                _attemptActive = false;
                _rxBuffer.Clear();

                // Primeira tentativa imediata; depois, em caso de falha, a cada 3s
                _nextAttemptAtUtc = DateTime.UtcNow;

                StartLinkLoop_NoLock();
            }
        }

        private void OnTransportError(string[] msg)
        {
            Error?.Invoke(msg);
        }

        private void OnTransportBytesReceived(byte[] data)
        {
            BytesReceived?.Invoke(data);

            // Durante handshake: só processa ASCII/banner
            if (State != LinkState.Linked)
            {
                HandleHandshakeBytes(data);
                return;
            }

            // Após Linked: bytes pertencem ao SD-GW-LINK (COBS/CRC)
            if (_engine != null)
                _engine.OnBytesReceived(data);
        }

        #endregion

        // =========================================================
        // Seção: Link / Handshake (drain + banner + detecção do OK)
        // =========================================================
        #region Link Handshake

        private readonly object _linkSync = new object();
        private System.Threading.Timer _linkTimer;

        private readonly StringBuilder _rxBuffer = new StringBuilder();

        private DateTime _drainUntil;
        private bool _draining;

        // Controle de tentativas
        private bool _attemptActive;
        private DateTime _attemptDeadlineUtc;
        private DateTime _nextAttemptAtUtc;

        // Timings conforme seu requisito
        private static readonly TimeSpan DrainWindow = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromMilliseconds(2000);
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(3);

        private const string API_BANNER = "\nSIMULDIESELAPI\n";
        private const string ESP_OK_PREFIX = "SimulDIESEL ver";

        private void StartLinkLoop_NoLock()
        {
            // Pré-condição: já está dentro de lock(_linkSync)
            if (_linkTimer == null)
                _linkTimer = new System.Threading.Timer(LinkTick, null, 50, 50);

            _linkTimer.Change(50, 50);
        }

        private void LinkTick(object state)
        {
            lock (_linkSync)
            {
                if (_disposed || !_transport.IsOpen)
                    return;

                if (State == LinkState.Linked)
                    return;

                var now = DateTime.UtcNow;

                // Ainda não é hora da próxima tentativa
                if (!_attemptActive && now < _nextAttemptAtUtc)
                    return;

                // Início de uma tentativa
                if (!_attemptActive)
                {
                    _rxBuffer.Clear();

                    _draining = true;
                    _drainUntil = now.Add(DrainWindow);

                    _attemptDeadlineUtc = now.Add(HandshakeTimeout);
                    _attemptActive = true;

                    SetState(LinkState.Draining);
                    return;
                }

                // Fim da janela de DRAIN => envia banner
                if (_draining && now >= _drainUntil)
                {
                    _draining = false;

                    var bannerBytes = Encoding.ASCII.GetBytes(API_BANNER);
                    _transport.Write(bannerBytes);

                    Console.WriteLine(" SerialLinkService. Banner enviado.");
                    SetState(LinkState.BannerSent);
                }

                // Timeout da tentativa atual (NÃO desconecta serial)
                if (now >= _attemptDeadlineUtc && State != LinkState.Linked)
                {
                    Console.WriteLine(" SerialLinkService. Handshake timeout (tentativa falhou).");

                    SetState(LinkState.LinkFailed);

                    _attemptActive = false;
                    _nextAttemptAtUtc = now.Add(RetryInterval); // tenta de novo em 3s
                }
            }
        }

        private void HandleHandshakeBytes(byte[] data)
        {
            if (!_transport.IsOpen)
                return;

            lock (_linkSync)
            {
                // Só processa se estivermos numa tentativa ativa
                if (!_attemptActive)
                    return;

                // só processa durante o handshake
                if (State != LinkState.Draining && State != LinkState.BannerSent)
                    return;

                // Durante DRAIN descarta tudo
                if (_draining)
                    return;

                string chunk = Encoding.ASCII.GetString(data);
                _rxBuffer.Append(chunk);

                while (true)
                {
                    string full = _rxBuffer.ToString();
                    int idx = full.IndexOf('\n');
                    if (idx < 0)
                        break;

                    string line = full.Substring(0, idx).Trim('\r');
                    _rxBuffer.Remove(0, idx + 1);

                    if (string.IsNullOrWhiteSpace(line))
                        continue;

                    Console.WriteLine($" SerialLinkService RX: '{line}'");

                    if (line.IndexOf(ESP_OK_PREFIX, StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        SetState(LinkState.Linked);

                        // Link estabelecido: para as tentativas
                        _attemptActive = false;
                        StopLinkTimer();

                        return;
                    }
                }
            }
        }

        private void StopLinkTimer()
        {
            _linkTimer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        #endregion
    }
}
