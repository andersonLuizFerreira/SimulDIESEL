using System;
using System.IO.Ports;
using System.Text;
using SimulDIESEL.DAL;

namespace SimulDIESEL.BLL
{
    public sealed class SerialLinkService : IDisposable
    {
        private readonly SerialTransport _transport;
        private bool _disposed;

        private SdGwLinkEngine _engine;
        private SdGwHealthService _health;

        private volatile bool _isDisconnecting;


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
        public event Action<bool> ConnectionChanged;
        public event Action<string[]> Error;
        public event Action<byte[]> BytesReceived;
        public event Action NomeDaInterfaceChanged;

        public LinkState State { get; private set; } = LinkState.Disconnected;

        public string NomeDaInterface { get; private set; } = "Nenhum";

        public bool IsLinked => State == LinkState.Linked;
        public bool IsConnected => _transport.IsOpen;
        public bool IsSerialOpen => _transport.IsOpen;

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

            _transport.ConnectionChanged -= OnTransportConnectionChanged;
            _transport.Error -= OnTransportError;
            _transport.BytesReceived -= OnTransportBytesReceived;

            _transport.ConnectionChanged += OnTransportConnectionChanged;
            _transport.Error += OnTransportError;
            _transport.BytesReceived += OnTransportBytesReceived;

            var cfg = new SdGwLinkEngine.Config
            {
                CmdAck = 0xF1,
                CmdErr = 0xF2,
                FlagAckReq = 0x01,
                FlagIsEvt = 0x02,
                MaxRawFrameLen = 250
            };
            _engine = new SdGwLinkEngine(cfg, WriteRaw);

            var healthCfg = new SdGwHealthService.Config
            {
                PingCmd = 0x55,
                PingIntervalMs = 1000,
                PingTimeoutMs = 150,
                PingRetries = 2
            };
            _health = new SdGwHealthService(_engine, healthCfg);

            _health.LinkHealthChanged -= Health_LinkHealthChanged;
            _health.LinkHealthChanged += Health_LinkHealthChanged;

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
            if (_disposed) return;

            // health só manda quando estava Linked
            if (State != LinkState.Linked) return;

            // se já caiu transporte, não faz nada aqui (o ConnectionChanged(false) vai resolver)
            if (!_transport.IsOpen) return;

            if (!alive)
            {
                Console.WriteLine(" SerialLinkService. Health DEAD => Link caiu. Reiniciando tentativa de link.");

                lock (_linkSync)
                {
                    SetState(LinkState.LinkFailed);

                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();

                    _nextAttemptAtUtc = DateTime.UtcNow;
                    StartLinkLoop_NoLock();
                }
            }
        }

        public static string[] ListarPortas() => SerialTransport.ListPorts();

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            StopLinkTimer();

            _transport.ConnectionChanged -= OnTransportConnectionChanged;
            _transport.Error -= OnTransportError;
            _transport.BytesReceived -= OnTransportBytesReceived;

            if (_health != null) _health.LinkHealthChanged -= Health_LinkHealthChanged;
            LinkStateChanged -= OnLinkStateChanged_ForHealth;

            if (_health != null) _health.Dispose();
            _health = null;
            _engine = null;

            _transport.Dispose();
        }

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

            _isDisconnecting = true;
            try
            {
                // 1) Mata tudo que pode gerar TX, antes de fechar a porta
                lock (_linkSync)
                {
                    _health?.SetEnabled(false);

                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();

                    StopLinkTimer();
                    SetState(LinkState.Disconnected);

                    if (NomeDaInterface != "Nenhum")
                    {
                        NomeDaInterface = "Nenhum";
                    }
                }

                // 2) Cancela qualquer SendAsync pendente e limpa RX do engine
                _engine?.OnTransportDown("Disconnect requested");

                // 3) Agora fecha o transporte
                _transport.Disconnect();

                // 4) Notifica nome (fora do lock)
                NomeDaInterfaceChanged?.Invoke();
            }
            finally
            {
                _isDisconnecting = false;
            }
        }


        public bool WriteRaw(byte[] data)
        {
            if (_disposed) return false;
            return _transport.Write(data);
        }

        private void OnTransportConnectionChanged(bool connected)
        {
            ConnectionChanged?.Invoke(connected);
            Console.WriteLine($" SerialLinkService. ConnectionChanged Invoked. Connected={connected}");

            bool raiseNomeChanged = false;

            lock (_linkSync)
            {
                if (!connected)
                {
                    if (_isDisconnecting)
                    {
                        // Já derrubamos estado/timers no Disconnect(); não precisa refazer tudo nem logar
                        return;
                    }

                    // Desliga health imediatamente
                    _health?.SetEnabled(false);

                    // Informa o LinkEngine que o transporte caiu (encerra SendAsync pendentes)
                    _engine?.OnTransportDown("Serial transport disconnected");

                    // Para handshake completamente e limpa estado
                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();

                    StopLinkTimer();
                    SetState(LinkState.Disconnected);

                    // Limpa nome da interface
                    if (NomeDaInterface != "Nenhum")
                    {
                        NomeDaInterface = "Nenhum";
                        raiseNomeChanged = true;
                    }
                }
                else
                {
                    // Transporte subiu
                    SetState(LinkState.SerialConnected);

                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();

                    _nextAttemptAtUtc = DateTime.UtcNow;
                    StartLinkLoop_NoLock();
                }
            }

            // Disparar evento sempre fora do lock
            if (raiseNomeChanged)
                NomeDaInterfaceChanged?.Invoke();
        }


        private void OnTransportError(string[] msg)
        {
            Error?.Invoke(msg);
        }

        private void OnTransportBytesReceived(byte[] data)
        {
            BytesReceived?.Invoke(data);

            if (State != LinkState.Linked)
            {
                HandleHandshakeBytes(data);
                return;
            }

            if (_engine != null)
                _engine.OnBytesReceived(data);
        }

        #endregion

        #region Link Handshake

        private readonly object _linkSync = new object();
        private System.Threading.Timer _linkTimer;
        private readonly StringBuilder _rxBuffer = new StringBuilder();

        private DateTime _drainUntil;
        private bool _draining;

        private bool _attemptActive;
        private DateTime _attemptDeadlineUtc;
        private DateTime _nextAttemptAtUtc;

        private static readonly TimeSpan DrainWindow = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromMilliseconds(2000);
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(3);

        private const string API_BANNER = "\nSIMULDIESELAPI\n";
        private const string ESP_OK_PREFIX = "SimulDIESEL ver";

        private void StartLinkLoop_NoLock()
        {
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

                if (!_attemptActive && now < _nextAttemptAtUtc)
                    return;

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

                if (_draining && now >= _drainUntil)
                {
                    _draining = false;

                    var bannerBytes = Encoding.ASCII.GetBytes(API_BANNER);
                    _transport.Write(bannerBytes);

                    Console.WriteLine(" SerialLinkService. Banner enviado.");
                    SetState(LinkState.BannerSent);
                }

                if (now >= _attemptDeadlineUtc && State != LinkState.Linked)
                {
                    Console.WriteLine(" SerialLinkService. Handshake timeout (tentativa falhou).");

                    SetState(LinkState.LinkFailed);

                    _attemptActive = false;
                    _nextAttemptAtUtc = now.Add(RetryInterval);
                }
            }
        }

        private void HandleHandshakeBytes(byte[] data)
        {
            if (!_transport.IsOpen)
                return;

            bool raiseNomeChanged = false;

            lock (_linkSync)
            {
                if (!_attemptActive)
                    return;

                if (State != LinkState.Draining && State != LinkState.BannerSent)
                    return;

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

                        _attemptActive = false;
                        StopLinkTimer();

                        NomeDaInterface = line;
                        raiseNomeChanged = true;
                        break;
                    }
                }
            }

            if (raiseNomeChanged)
                NomeDaInterfaceChanged?.Invoke();
        }

        private void StopLinkTimer()
        {
            _linkTimer?.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
        }

        #endregion
    }
}
