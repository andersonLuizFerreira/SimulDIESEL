using System;
using System.IO.Ports;
using System.Text;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM.Backplane;
using SimulDIESEL.BLL.Boards.BPM.XConn;
using SimulDIESEL.BLL.Boards.GSA;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DAL.Transport.Serial;
using SimulDIESEL.DTL.Common;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.BPM.Comm.Serial
{
    /// <summary>
    /// Fachada funcional da comunicação serial da BPM.
    /// A BPM continua dona do fluxo funcional; transporte e protocolo técnico
    /// permanecem delegados à DAL.
    /// </summary>
    public class BpmSerialService : IDisposable
    {
        private static readonly Lazy<BpmSerialService> SharedInstance = new Lazy<BpmSerialService>(() => new BpmSerialService());

        private readonly SerialTransport _transport;
        private readonly object _linkSync = new object();
        private readonly StringBuilder _rxBuffer = new StringBuilder();

        private bool _disposed;
        private volatile bool _isDisconnecting;
        private SdGwLinkEngine _engine;
        private SdGwTxScheduler _txScheduler;
        private SdGwLinkSupervisor _linkSupervisor;
        private System.Threading.Timer _linkTimer;
        private DateTime _drainUntil;
        private bool _draining;
        private bool _attemptActive;
        private DateTime _attemptDeadlineUtc;
        private DateTime _nextAttemptAtUtc;
        private volatile bool _sdgwSessionEstablished;

        private static readonly TimeSpan DrainWindow = TimeSpan.FromMilliseconds(300);
        private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromMilliseconds(2000);
        private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(3);

        private const string ApiBanner = "\nSIMULDIESELAPI\n";

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

        public SdgwSession Sdgw { get; private set; }

        // Alias legado transitório para compatibilidade com nomenclatura antiga.
        public SdgwSession Sggw => Sdgw;
        public SdhClient Sdh { get; private set; }
        public GsaClient Gsa { get; private set; }
        public BpmClient Bpm { get; private set; }
        public BackplaneService Backplane { get; private set; }
        public XConnService XConn { get; private set; }
        public Comm.Bluetooth.BpmBluetoothService Bluetooth { get; private set; }
        public Comm.Network.BpmNetworkService Network { get; private set; }

        public static BpmSerialService Shared => SharedInstance.Value;

        public bool IsLinked => State == LinkState.Linked;
        public bool IsConnected => _transport.IsOpen;
        public bool IsSerialOpen => _transport.IsOpen;

        public BpmSerialService()
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
            _txScheduler = new SdGwTxScheduler(_engine);
            Sdgw = new SdgwSession(_engine, _txScheduler);
            Sdh = new SdhClient(Sdgw);
            Backplane = new BackplaneService();
            XConn = new XConnService();
            Bluetooth = new Comm.Bluetooth.BpmBluetoothService();
            Network = new Comm.Network.BpmNetworkService();
            Gsa = new GsaClient(Sdh, Sdgw);
            Bpm = new BpmClient(Sdh, this, Backplane, XConn);

            var linkSupervisorCfg = new SdGwLinkSupervisor.Config
            {
                PingCmd = 0x55,
                IdleBeforePingMs = 1500,
                LinkTimeoutMs = 3000,
                PingTimeoutMs = 150,
                PingRetries = 2,
                TickPeriodMs = 50
            };

            _linkSupervisor = new SdGwLinkSupervisor(linkSupervisorCfg, SendSupervisorPingAsync);
            _linkSupervisor.LinkHealthChanged -= LinkSupervisor_LinkHealthChanged;
            _linkSupervisor.LinkHealthChanged += LinkSupervisor_LinkHealthChanged;

            _engine.ValidFrameReceived -= Engine_ValidFrameReceived;
            _engine.ValidFrameReceived += Engine_ValidFrameReceived;

            LinkStateChanged -= OnLinkStateChanged_ForLinkSupervisor;
            LinkStateChanged += OnLinkStateChanged_ForLinkSupervisor;
        }

        public static string[] ListarPortas()
        {
            return SerialTransport.ListPorts();
        }

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
            if (_disposed)
                throw new ObjectDisposedException(nameof(BpmSerialService));

            return _transport.Connect(
                portName: portName,
                baudRate: baudRate,
                parity: parity,
                dataBits: dataBits,
                stopBits: stopBits,
                handshake: handshake,
                dtrEnable: dtrEnable,
                rtsEnable: rtsEnable);
        }

        public void Disconnect()
        {
            if (_disposed)
                return;

            _isDisconnecting = true;
            try
            {
                bool shouldSendLogout = false;

                lock (_linkSync)
                {
                    _linkSupervisor?.SetEnabled(false);
                    _sdgwSessionEstablished = false;
                    shouldSendLogout = _transport.IsOpen && Sdgw != null && State == LinkState.Linked;
                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();
                    StopAndDisposeLinkTimer_NoLock();
                    SetState(LinkState.Disconnected);
                    NomeDaInterface = "Nenhum";
                }

                if (shouldSendLogout)
                {
                    try
                    {
                        Sdgw.SendAsync(
                            SggwCmd.LOGOUT,
                            requireAck: true,
                            timeoutMs: 150,
                            retries: 1,
                            priority: SdGwTxPriority.High,
                            origin: "SGGW logout").Wait(500);
                    }
                    catch
                    {
                    }
                }

                _txScheduler?.SetTransportAvailable(false);
                _engine?.OnTransportDown("Disconnect requested");
                _transport.Disconnect();
                NomeDaInterfaceChanged?.Invoke();
            }
            finally
            {
                _isDisconnecting = false;
            }
        }

        public bool WriteRaw(byte[] data)
        {
            if (_disposed)
                return false;

            return _transport.Write(data);
        }

        private Task<SdGwLinkEngine.SendOutcome> SendSupervisorPingAsync()
        {
            if (_txScheduler == null)
                return Task.FromResult(SdGwLinkEngine.SendOutcome.TransportDown);

            return _txScheduler.EnqueueAsync(
                cmd: 0x55,
                payload: Array.Empty<byte>(),
                options: new SdGwLinkEngine.SendOptions
                {
                    RequireAck = true,
                    TimeoutMs = 150,
                    MaxRetries = 2,
                    IsEvent = false
                },
                priority: SdGwTxPriority.Low,
                origin: "SDGW link supervisor ping");
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            lock (_linkSync)
            {
                StopAndDisposeLinkTimer_NoLock();
            }

            _txScheduler?.SetTransportAvailable(false);
            _engine?.OnTransportDown("BpmSerialService dispose");

            _transport.ConnectionChanged -= OnTransportConnectionChanged;
            _transport.Error -= OnTransportError;
            _transport.BytesReceived -= OnTransportBytesReceived;

            if (_engine != null)
                _engine.ValidFrameReceived -= Engine_ValidFrameReceived;

            if (_linkSupervisor != null)
                _linkSupervisor.LinkHealthChanged -= LinkSupervisor_LinkHealthChanged;

            LinkStateChanged -= OnLinkStateChanged_ForLinkSupervisor;

            if (_linkSupervisor != null)
                _linkSupervisor.Dispose();

            _linkSupervisor = null;
            _txScheduler?.Dispose();
            _txScheduler = null;

            if (Gsa != null)
                Gsa.Dispose();

            Gsa = null;
            Sdgw = null;
            _engine = null;
            _transport.Dispose();
        }

        private void SetState(LinkState newState)
        {
            if (State == newState)
                return;

            State = newState;
            LinkStateChanged?.Invoke(State);
        }

        private void OnLinkStateChanged_ForLinkSupervisor(LinkState state)
        {
            _linkSupervisor?.SetEnabled(state == LinkState.Linked);
        }

        private void LinkSupervisor_LinkHealthChanged(bool alive)
        {
            if (_disposed || _isDisconnecting || State != LinkState.Linked || !_transport.IsOpen || alive)
                return;

            lock (_linkSync)
            {
                if (_isDisconnecting)
                    return;

                SetState(LinkState.LinkFailed);
                _attemptActive = false;
                _draining = false;
                _rxBuffer.Clear();
                _nextAttemptAtUtc = DateTime.UtcNow;
                StartLinkLoop_NoLock();
            }
        }

        private void OnTransportConnectionChanged(bool connected)
        {
            ConnectionChanged?.Invoke(connected);
            bool raiseNomeChanged = false;

            lock (_linkSync)
            {
                if (!connected)
                {
                    if (_isDisconnecting)
                        return;

                    _linkSupervisor?.SetEnabled(false);
                    _sdgwSessionEstablished = false;
                    _txScheduler?.SetTransportAvailable(false);
                    _engine?.OnTransportDown("Serial transport disconnected");
                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();
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
                    _sdgwSessionEstablished = false;
                    _txScheduler?.SetTransportAvailable(true);
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

        private void Engine_ValidFrameReceived()
        {
            _linkSupervisor?.OnValidFrameReceived();
        }

        private void OnTransportBytesReceived(byte[] data)
        {
            BytesReceived?.Invoke(data);

            bool sdgwSessionEstablished = _sdgwSessionEstablished;

            if (sdgwSessionEstablished)
            {
                if (LooksLikeAsciiTextNoise(data))
                {
                    if (State != LinkState.Linked)
                        HandleHandshakeBytes(data);

                    return;
                }

                _engine?.OnBytesReceived(data);
                return;
            }

            if (State == LinkState.Linked)
            {
                if (LooksLikeAsciiTextNoise(data))
                    return;

                _engine?.OnBytesReceived(data);
                return;
            }

            HandleHandshakeBytes(data);
        }

        private static bool LooksLikeAsciiTextNoise(byte[] data)
        {
            if (data == null || data.Length == 0)
                return false;

            for (int i = 0; i < data.Length; i++)
            {
                byte value = data[i];
                if (value == 0x00)
                    return false;

                bool ok =
                    value == (byte)'\r' ||
                    value == (byte)'\n' ||
                    value == (byte)'\t' ||
                    (value >= 32 && value <= 126);

                if (!ok)
                    return false;
            }

            return true;
        }

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
                if (_disposed || !_transport.IsOpen || State == LinkState.Linked)
                    return;

                DateTime now = DateTime.UtcNow;
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
                    _transport.Write(Encoding.ASCII.GetBytes(ApiBanner));
                    SetState(LinkState.BannerSent);
                }

                if (now >= _attemptDeadlineUtc && State != LinkState.Linked)
                {
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
                if (!_attemptActive || (State != LinkState.Draining && State != LinkState.BannerSent) || _draining)
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

                    DeviceInfo deviceInfo;
                    if (BpmParsers.TryParseInterfaceInfo(line, out deviceInfo))
                    {
                        _sdgwSessionEstablished = true;
                        SetState(LinkState.Linked);
                        _attemptActive = false;
                        StopAndDisposeLinkTimer_NoLock();
                        NomeDaInterface = deviceInfo.Version;
                        raiseNomeChanged = true;
                        break;
                    }
                }
            }

            if (raiseNomeChanged)
                NomeDaInterfaceChanged?.Invoke();
        }

        private void StopAndDisposeLinkTimer_NoLock()
        {
            if (_linkTimer == null)
                return;

            try { _linkTimer.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite); } catch { }
            try { _linkTimer.Dispose(); } catch { }
            _linkTimer = null;
        }
    }
}
