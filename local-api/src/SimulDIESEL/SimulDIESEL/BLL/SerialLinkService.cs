using System;
using System.IO.Ports;
using System.Text;
using SimulDIESEL.DAL;
using SimulDIESEL.DTL;

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

        public SdGgwClient Sggw { get; private set; }

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

            Sggw = new SdGgwClient(_engine);

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

            // se estou desconectando manualmente, ignore health
            if (_isDisconnecting) return;

            if (State != LinkState.Linked) return;
            if (!_transport.IsOpen) return;

            if (!alive)
            {
                Console.WriteLine(" SerialLinkService. Health DEAD => Link caiu. Reiniciando tentativa de link.");

                lock (_linkSync)
                {
                    if (_isDisconnecting) return;

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

            lock (_linkSync)
            {
                StopAndDisposeLinkTimer_NoLock();
            }

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
                bool raiseNomeChanged = false;
                bool shouldSendLogout = false;

                lock (_linkSync)
                {
                    _health?.SetEnabled(false);

                    shouldSendLogout = _transport.IsOpen && Sggw != null && State == LinkState.Linked;

                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();

                    // (5) para e DISPOSE do timer (parada definitiva)
                    StopAndDisposeLinkTimer_NoLock();

                    SetState(LinkState.Disconnected);

                    if (NomeDaInterface != "Nenhum")
                    {
                        NomeDaInterface = "Nenhum";
                        raiseNomeChanged = true;
                    }
                }

                // tenta LOGOUT (opcional)
                if (shouldSendLogout)
                {
                    try
                    {
                        var ticket = Sggw.SendWithSeq(SggwCmd.LOGOUT, requireAck: true, timeoutMs: 150, retries: 1);
                        Console.WriteLine($" SerialLinkService. LOGOUT enviado. seq={ticket.Seq}");

                        if (ticket.Task.Wait(500))
                        {
                            var outcome = ticket.Task.Result;
                            Console.WriteLine($" SerialLinkService. LOGOUT result. seq={ticket.Seq} outcome={outcome}");
                        }
                        else
                        {
                            Console.WriteLine($" SerialLinkService. LOGOUT wait timeout. seq={ticket.Seq}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(" SerialLinkService. LOGOUT send failed: " + ex.Message);
                    }
                }

                // cancela pendências do engine
                _engine?.OnTransportDown("Disconnect requested");

                // fecha o transporte
                _transport.Disconnect();

                if (raiseNomeChanged)
                    NomeDaInterfaceChanged?.Invoke();
                else
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
                        return;

                    _health?.SetEnabled(false);

                    _engine?.OnTransportDown("Serial transport disconnected");

                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();

                    // (5) como o transporte caiu, podemos parar e DISPOSE do timer
                    StopAndDisposeLinkTimer_NoLock();

                    SetState(LinkState.Disconnected);

                    if (NomeDaInterface != "Nenhum")
                    {
                        NomeDaInterface = "Nenhum";
                        raiseNomeChanged = true;
                    }
                }
                else
                {
                    SetState(LinkState.SerialConnected);

                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();

                    _nextAttemptAtUtc = DateTime.UtcNow;
                    StartLinkLoop_NoLock();
                }
            }

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

            // (6) Se já está Linked, mas o firmware ainda envia texto/log ASCII,
            // descartamos esse "ruído" para não poluir o SdGwLinkEngine (CRC/overflow).
            if (State == LinkState.Linked)
            {
                if (LooksLikeAsciiTextNoise(data))
                    return;

                if (_engine != null)
                    _engine.OnBytesReceived(data);

                return;
            }

            // não linked: bytes pertencem ao handshake textual
            HandleHandshakeBytes(data);
        }

        // (6) filtro conservador de ruído ASCII:
        // - não contém 0x00 (delimitador do COBS)
        // - todos bytes são ASCII imprimíveis/controle comum (\r \n \t)
        private static bool LooksLikeAsciiTextNoise(byte[] data)
        {
            if (data == null || data.Length == 0) return false;

            for (int i = 0; i < data.Length; i++)
            {
                byte b = data[i];
                if (b == 0x00) return false; // parece frame binário (delimitador presente)

                bool ok =
                    b == (byte)'\r' ||
                    b == (byte)'\n' ||
                    b == (byte)'\t' ||
                    (b >= 32 && b <= 126);

                if (!ok) return false; // tem byte não-ASCII -> não tratar como "texto"
            }

            return true;
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
            else
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
                        // IMPORTANTE: após Linked, o firmware IDEALMENTE deve parar logs texto
                        // e operar 100% em frames COBS+CRC.
                        SetState(LinkState.Linked);

                        _attemptActive = false;

                        // (5) Linked: para e DISPOSE do timer (não precisa mais)
                        StopAndDisposeLinkTimer_NoLock();

                        NomeDaInterface = line;
                        raiseNomeChanged = true;
                        break;
                    }
                }
            }

            if (raiseNomeChanged)
                NomeDaInterfaceChanged?.Invoke();
        }

        // (5) Parada definitiva do timer (evita callback zumbi / vazamento)
        private void StopAndDisposeLinkTimer_NoLock()
        {
            if (_linkTimer != null)
            {
                try { _linkTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); } catch { }
                try { _linkTimer.Dispose(); } catch { }
                _linkTimer = null;
            }
        }

        #endregion
    }
}
