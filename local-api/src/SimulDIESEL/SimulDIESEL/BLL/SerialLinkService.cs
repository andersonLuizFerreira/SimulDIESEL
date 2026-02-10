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
        private readonly SerialTransport _transport; // acesso protegido por _disposed
        
        private bool _disposed;

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
        
        public bool IsConnected => IsLinked; // compatibilidade com UI
        
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

            // evita múltiplas inscrições (defensivo)
            _transport.ConnectionChanged -= OnTransportConnectionChanged;
            _transport.Error -= OnTransportError;
            _transport.BytesReceived -= OnTransportBytesReceived;

            _transport.ConnectionChanged += OnTransportConnectionChanged;
            _transport.Error += OnTransportError;
            _transport.BytesReceived += OnTransportBytesReceived;
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

            _transport.ConnectionChanged -= OnTransportConnectionChanged;
            _transport.Error -= OnTransportError;
            _transport.BytesReceived -= OnTransportBytesReceived;

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

        private void OnTransportConnectionChanged(bool connected)
        {
            ConnectionChanged?.Invoke(connected);

            Console.WriteLine($" SerialLinkService. ConnectionChanged Invoked. Connected={connected}");

            if (!connected)
            {
                StopLinkTimer();
                SetState(LinkState.Disconnected);
                return;
            }

            SetState(LinkState.SerialConnected);
            StartHandshake();
        }

        private void OnTransportError(string[] msg)
        {
            Error?.Invoke(msg);
        }

        private void OnTransportBytesReceived(byte[] data)
        {
            BytesReceived?.Invoke(data);
            HandleHandshakeBytes(data); // delega para a seção de link
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
        
        private DateTime _linkTimeoutUntil;
        
        private bool _draining;

        private const string API_BANNER = "\nSIMULDIESELAPI\n";
        
        private const string ESP_OK_PREFIX = "SimulDIESEL ver";

        private void StartHandshake()
        {
            lock (_linkSync)
            {
                _rxBuffer.Clear();

                // 300ms drenando lixo
                _draining = true;
                _drainUntil = DateTime.UtcNow.AddMilliseconds(300);

                // timeout total do handshake (2 segundos)
                _linkTimeoutUntil = DateTime.UtcNow.AddMilliseconds(2000);

                SetState(LinkState.Draining);

                if (_linkTimer == null)
                    _linkTimer = new System.Threading.Timer(LinkTick, null, 50, 50);

                _linkTimer.Change(50, 50);
            }
        }

        private void LinkTick(object state)
        {
            lock (_linkSync)
            {
                if (_disposed || !_transport.IsOpen)
                    return;

                var now = DateTime.UtcNow;

                // Fim da janela de DRAIN
                if (_draining && now >= _drainUntil)
                {
                    _draining = false;

                    var bannerBytes = Encoding.ASCII.GetBytes(API_BANNER);
                    _transport.Write(bannerBytes);

                    Console.WriteLine(" SerialLinkService. Banner enviado.");
                    SetState(LinkState.BannerSent);
                }

                // Timeout geral
                if (now >= _linkTimeoutUntil && State != LinkState.Linked)
                {
                    Console.WriteLine(" SerialLinkService. Handshake timeout.");

                    SetState(LinkState.LinkFailed);
                    _transport.Disconnect();
                }
            }
        }

        private void HandleHandshakeBytes(byte[] data)
        {
            if (!_transport.IsOpen)
                return;

            lock (_linkSync)
            {
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
