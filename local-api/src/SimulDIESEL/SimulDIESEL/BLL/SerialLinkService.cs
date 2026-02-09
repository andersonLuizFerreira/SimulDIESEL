using System;
using System.IO.Ports;
using System.Linq;
using System.Text;
using SimulDIESEL.DAL;

namespace SimulDIESEL.BLL
{
    /// <summary>
    /// BLL mínima para conexão serial (sem tráfego/protocolo).
    /// UI fala com esta classe. Ela usa a DAL (SerialTransport).
    /// </summary>
    public sealed class SerialLinkService : IDisposable
    {
        private readonly SerialTransport _transport; // acesso protegido por _disposed
        
        private bool _disposed; // acesso protegido por _disposed
        
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

        private readonly object _linkSync = new object();
        
        private System.Threading.Timer _linkTimer;

        private readonly StringBuilder _rxBuffer = new StringBuilder();

        private DateTime _drainUntil;
        
        private DateTime _linkTimeoutUntil;

        private bool _draining;

        private const string API_BANNER = "\nSIMULDIESELAPI\n";
        
        private const string ESP_OK_PREFIX = "SimulDIESEL ver";

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

            _transport.ConnectionChanged -= OnConnectionChanged; // para evitar múltiplas inscrições se criar mais de um SerialLinkService (não deveria, mas só pra garantir)
            _transport.ConnectionChanged += OnConnectionChanged; // repassa para UI

            _transport.Error -= OnError; // para evitar múltiplas inscrições se criar mais de um SerialLinkService (não deveria, mas só pra garantir)
            _transport.Error += OnError; // repassa para UI

            _transport.BytesReceived -= OnBytesReceived; // para evitar múltiplas inscrições se criar mais de um SerialLinkService (não deveria, mas só pra garantir)
            _transport.BytesReceived += OnBytesReceived; // por enquanto, só repassa (ou nem usa)
        }

        public bool IsSerialOpen => _transport.IsOpen;
        
        public bool IsConnected => IsLinked;

        public static string[] ListarPortas() // repassa para UI
        {
            return SerialTransport.ListPorts();
        }

        public event Action<bool> ConnectionChanged;
        
        public event Action<string[]> Error;
        
        public event Action<byte[]> BytesReceived; // opcional (debug), não vamos usar tráfego ainda

        public bool Connect(string portName, int baudRate,
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

        private void OnConnectionChanged(bool connected)
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
                {
                    _linkTimer = new System.Threading.Timer(LinkTick, null, 50, 50);
                }
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

        private void OnError(string[] msg)
        {
            Error?.Invoke(msg);
        }

        private void OnBytesReceived(byte[] data)
        {
            BytesReceived?.Invoke(data);

            if (!_transport.IsOpen)
                return;

            lock (_linkSync)
            {
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

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _transport.ConnectionChanged -= OnConnectionChanged;
            _transport.Error -= OnError;
            _transport.BytesReceived -= OnBytesReceived;

            _transport.Dispose();
        }
    }
}
