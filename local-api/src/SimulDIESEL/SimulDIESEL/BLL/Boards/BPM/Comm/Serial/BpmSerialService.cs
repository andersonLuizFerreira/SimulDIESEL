using System;
using System.IO.Ports;
using SimulDIESEL.BLL.Boards.BPM.Backplane;
using SimulDIESEL.BLL.Boards.BPM.XConn;
using SimulDIESEL.BLL.Boards.GSA;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DAL.Transport.Serial;

namespace SimulDIESEL.BLL.Boards.BPM.Comm.Serial
{
    /// <summary>
    /// Fachada legada da comunicação serial da BPM.
    /// Mantida para compatibilidade com UI/BLL atual, mas delegando a
    /// sessão SDGW para um núcleo genérico independente do transporte.
    /// </summary>
    public class BpmSerialService : IDisposable
    {
        private static readonly Lazy<BpmSerialService> SharedInstance = new Lazy<BpmSerialService>(() => new BpmSerialService());

        private readonly SerialTransport _transport;
        private readonly Comm.SdgwHostSession _session;
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
        public event Action<bool> ConnectionChanged;
        public event Action<string[]> Error;
        public event Action<byte[]> BytesReceived;
        public event Action NomeDaInterfaceChanged;

        public LinkState State { get; private set; } = LinkState.Disconnected;
        public string NomeDaInterface => _session != null ? _session.InterfaceName : "Nenhum";

        public SdgwSession Sdgw => _session != null ? _session.Sdgw : null;

        // Alias legado transitório para compatibilidade com nomenclatura antiga.
        public SdgwSession Sggw => Sdgw;
        public SdhClient Sdh => _session != null ? _session.Sdh : null;
        public GsaClient Gsa { get; private set; }
        public BpmClient Bpm { get; private set; }
        public BackplaneService Backplane { get; private set; }
        public XConnService XConn { get; private set; }
        public Comm.Bluetooth.BpmBluetoothService Bluetooth { get; private set; }
        public Comm.Network.BpmNetworkService Network { get; private set; }

        public static BpmSerialService Shared => SharedInstance.Value;

        public bool IsLinked => _session != null && _session.IsLinked;
        public bool IsConnected => _session != null && _session.IsConnected;
        public bool IsSerialOpen => IsConnected;

        public BpmSerialService()
        {
            _transport = new SerialTransport();
            _session = new Comm.SdgwHostSession(_transport);

            _session.ConnectionChanged += OnSessionConnectionChanged;
            _session.Error += OnSessionError;
            _session.BytesReceived += OnSessionBytesReceived;
            _session.InterfaceNameChanged += OnSessionInterfaceNameChanged;
            _session.StateChanged += OnSessionStateChanged;

            Backplane = new BackplaneService();
            XConn = new XConnService();
            Bluetooth = new Comm.Bluetooth.BpmBluetoothService();
            Network = new Comm.Network.BpmNetworkService();
            Gsa = new GsaClient(Sdh, Sdgw);
            Bpm = new BpmClient(Sdh, this, Backplane, XConn);
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

            _session.Disconnect();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;

            _session.ConnectionChanged -= OnSessionConnectionChanged;
            _session.Error -= OnSessionError;
            _session.BytesReceived -= OnSessionBytesReceived;
            _session.InterfaceNameChanged -= OnSessionInterfaceNameChanged;
            _session.StateChanged -= OnSessionStateChanged;

            if (Gsa != null)
                Gsa.Dispose();

            Gsa = null;
            Bpm = null;
            _session.Dispose();
        }

        private void SetState(LinkState newState)
        {
            if (State == newState)
                return;

            State = newState;
            LinkStateChanged?.Invoke(State);
        }

        private void OnSessionConnectionChanged(bool connected)
        {
            ConnectionChanged?.Invoke(connected);
        }

        private void OnSessionError(string[] msg)
        {
            Error?.Invoke(msg);
        }

        private void OnSessionBytesReceived(byte[] data)
        {
            BytesReceived?.Invoke(data);
        }

        private void OnSessionInterfaceNameChanged()
        {
            NomeDaInterfaceChanged?.Invoke();
        }

        private void OnSessionStateChanged(Comm.SdgwHostSession.SessionState state)
        {
            SetState(MapState(state));
        }

        private static LinkState MapState(Comm.SdgwHostSession.SessionState state)
        {
            switch (state)
            {
                case Comm.SdgwHostSession.SessionState.TransportConnected:
                    return LinkState.SerialConnected;
                case Comm.SdgwHostSession.SessionState.Draining:
                    return LinkState.Draining;
                case Comm.SdgwHostSession.SessionState.BannerSent:
                    return LinkState.BannerSent;
                case Comm.SdgwHostSession.SessionState.Linked:
                    return LinkState.Linked;
                case Comm.SdgwHostSession.SessionState.LinkFailed:
                    return LinkState.LinkFailed;
                default:
                    return LinkState.Disconnected;
            }
        }
    }
}
