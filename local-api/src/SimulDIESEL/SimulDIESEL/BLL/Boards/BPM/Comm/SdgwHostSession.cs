using System;
using System.Text;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.BPM;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DAL.Transport;
using SimulDIESEL.DTL.Common;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.BPM.Comm
{
    /// <summary>
    /// Núcleo genérico da sessão host SDGW.
    /// Orquestra handshake, supervisão e ciclo de vida do link sobre um
    /// transporte de bytes, sem depender diretamente de SerialPort.
    /// </summary>
    public sealed class SdgwHostSession : IDisposable
    {
        private readonly IByteTransport _transport;
        private readonly object _linkSync = new object();
        private readonly StringBuilder _rxBuffer = new StringBuilder();

        private bool _disposed;
        private volatile bool _isDisconnecting;
        private readonly SdGwLinkEngine _engine;
        private readonly SdGwTxScheduler _txScheduler;
        private readonly SdGwLinkSupervisor _linkSupervisor;
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

        public enum SessionState
        {
            Disconnected,
            TransportConnected,
            Draining,
            BannerSent,
            Linked,
            LinkFailed
        }

        public event Action<SessionState> StateChanged;
        public event Action<bool> ConnectionChanged;
        public event Action<string[]> Error;
        public event Action<byte[]> BytesReceived;
        public event Action InterfaceNameChanged;

        public SessionState State { get; private set; } = SessionState.Disconnected;
        public string InterfaceName { get; private set; } = "Nenhum";
        public TransportKind TransportKind => _transport.Kind;

        public SdgwSession Sdgw { get; private set; }
        public SdhClient Sdh { get; private set; }

        public bool IsLinked => State == SessionState.Linked;
        public bool IsConnected => _transport.IsOpen;

        public SdgwHostSession(IByteTransport transport)
        {
            _transport = transport ?? throw new ArgumentNullException(nameof(transport));

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
            _linkSupervisor.LinkHealthChanged += LinkSupervisor_LinkHealthChanged;
            _engine.ValidFrameReceived += Engine_ValidFrameReceived;
            StateChanged += OnStateChanged_ForLinkSupervisor;
        }

        public void Disconnect()
        {
            if (_disposed)
                return;

            _isDisconnecting = true;
            try
            {
                lock (_linkSync)
                {
                    _linkSupervisor?.SetEnabled(false);
                    _sdgwSessionEstablished = false;
                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();
                    StopAndDisposeLinkTimer_NoLock();
                    SetState(SessionState.Disconnected);
                    InterfaceName = "Nenhum";
                }

                _txScheduler?.SetTransportAvailable(false);
                _engine?.OnTransportDown("Disconnect requested");
                _transport.Disconnect();
                InterfaceNameChanged?.Invoke();
            }
            finally
            {
                _isDisconnecting = false;
            }
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
            _engine?.OnTransportDown("SdgwHostSession dispose");

            _transport.ConnectionChanged -= OnTransportConnectionChanged;
            _transport.Error -= OnTransportError;
            _transport.BytesReceived -= OnTransportBytesReceived;

            _engine.ValidFrameReceived -= Engine_ValidFrameReceived;
            _linkSupervisor.LinkHealthChanged -= LinkSupervisor_LinkHealthChanged;
            StateChanged -= OnStateChanged_ForLinkSupervisor;

            _linkSupervisor.Dispose();
            Sdgw.Dispose();
            _txScheduler.Dispose();
            _transport.Dispose();

            Sdgw = null;
            Sdh = null;
        }

        private bool WriteRaw(byte[] data)
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

        private void SetState(SessionState newState)
        {
            if (State == newState)
                return;

            State = newState;
            StateChanged?.Invoke(State);
        }

        private void OnStateChanged_ForLinkSupervisor(SessionState state)
        {
            _linkSupervisor?.SetEnabled(state == SessionState.Linked);
        }

        private void LinkSupervisor_LinkHealthChanged(bool alive)
        {
            if (_disposed || _isDisconnecting || State != SessionState.Linked || !_transport.IsOpen || alive)
                return;

            lock (_linkSync)
            {
                if (_isDisconnecting)
                    return;

                SetState(SessionState.LinkFailed);
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
            bool raiseInterfaceChanged = false;

            lock (_linkSync)
            {
                if (!connected)
                {
                    if (_isDisconnecting)
                        return;

                    _linkSupervisor?.SetEnabled(false);
                    _sdgwSessionEstablished = false;
                    _txScheduler?.SetTransportAvailable(false);
                    _engine?.OnTransportDown("Transport disconnected");
                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();
                    StopAndDisposeLinkTimer_NoLock();
                    SetState(SessionState.Disconnected);

                    if (InterfaceName != "Nenhum")
                    {
                        InterfaceName = "Nenhum";
                        raiseInterfaceChanged = true;
                    }
                }
                else
                {
                    _sdgwSessionEstablished = false;
                    _txScheduler?.SetTransportAvailable(true);
                    SetState(SessionState.TransportConnected);
                    _attemptActive = false;
                    _draining = false;
                    _rxBuffer.Clear();
                    _nextAttemptAtUtc = DateTime.UtcNow;
                    StartLinkLoop_NoLock();
                }
            }

            if (raiseInterfaceChanged)
                InterfaceNameChanged?.Invoke();
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
                    if (State != SessionState.Linked)
                        HandleHandshakeBytes(data);

                    return;
                }

                _engine.OnBytesReceived(data);
                return;
            }

            if (State == SessionState.Linked)
            {
                if (LooksLikeAsciiTextNoise(data))
                    return;

                _engine.OnBytesReceived(data);
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
                if (_disposed || !_transport.IsOpen || State == SessionState.Linked)
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
                    SetState(SessionState.Draining);
                    return;
                }

                if (_draining && now >= _drainUntil)
                {
                    _draining = false;
                    _transport.Write(Encoding.ASCII.GetBytes(ApiBanner));
                    SetState(SessionState.BannerSent);
                }

                if (now >= _attemptDeadlineUtc && State != SessionState.Linked)
                {
                    SetState(SessionState.LinkFailed);
                    _attemptActive = false;
                    _nextAttemptAtUtc = now.Add(RetryInterval);
                }
            }
        }

        private void HandleHandshakeBytes(byte[] data)
        {
            if (!_transport.IsOpen)
                return;

            bool raiseInterfaceChanged = false;

            lock (_linkSync)
            {
                if (!_attemptActive || (State != SessionState.Draining && State != SessionState.BannerSent) || _draining)
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
                        SetState(SessionState.Linked);
                        _attemptActive = false;
                        StopAndDisposeLinkTimer_NoLock();
                        InterfaceName = deviceInfo.Version;
                        raiseInterfaceChanged = true;
                        break;
                    }
                }
            }

            if (raiseInterfaceChanged)
                InterfaceNameChanged?.Invoke();
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
