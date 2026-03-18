using System;
using System.IO.Ports;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.DTL.Boards.BPM;

namespace SimulDIESEL.BLL.FormsLogic.BPM
{
    /// <summary>
    /// Orquestra a interação dos forms com a board BPM.
    /// A UI deve consultar estado e acionar conexão por aqui, sem navegar
    /// diretamente pela árvore de serviços do link serial.
    /// </summary>
    public sealed class FrmBpmLogic : IDisposable
    {
        private readonly BpmSerialService _serialService;
        private bool _disposed;

        public event Action StatusChanged;
        public event Action<string[]> Error;

        public FrmBpmLogic(BpmSerialService serialService)
        {
            _serialService = serialService ?? throw new ArgumentNullException(nameof(serialService));

            _serialService.ConnectionChanged += OnStatusChanged;
            _serialService.LinkStateChanged += OnLinkStateChanged;
            _serialService.NomeDaInterfaceChanged += OnStatusChanged;
            _serialService.Error += OnError;
        }

        public static FrmBpmLogic CreateDefault()
        {
            return new FrmBpmLogic(BpmSerialService.Shared);
        }

        public string[] ListarPortas()
        {
            return BpmSerialService.ListarPortas();
        }

        public BpmStatusDto GetStatus()
        {
            return _serialService.Bpm.GetStatus();
        }

        public string GetInterfaceDisplayName()
        {
            BpmStatusDto status = GetStatus();
            return status.IsLinked ? status.InterfaceName : "Nenhum";
        }

        public bool Connect(string portName, int baudRate)
        {
            if (string.IsNullOrWhiteSpace(portName))
                throw new ArgumentException("Porta COM é obrigatória.", nameof(portName));

            if (baudRate <= 0)
                throw new ArgumentOutOfRangeException(nameof(baudRate), "Baud rate deve ser maior que zero.");

            return _serialService.Connect(
                portName,
                baudRate,
                dtrEnable: false,
                rtsEnable: false,
                parity: Parity.None,
                dataBits: 8,
                stopBits: StopBits.One,
                handshake: Handshake.None);
        }

        public void Disconnect()
        {
            _serialService.Disconnect();
        }

        private void OnStatusChanged(bool _)
        {
            StatusChanged?.Invoke();
        }

        private void OnLinkStateChanged(BpmSerialService.LinkState _)
        {
            StatusChanged?.Invoke();
        }

        private void OnStatusChanged()
        {
            StatusChanged?.Invoke();
        }

        private void OnError(string[] msg)
        {
            Error?.Invoke(msg);
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _serialService.ConnectionChanged -= OnStatusChanged;
            _serialService.LinkStateChanged -= OnLinkStateChanged;
            _serialService.NomeDaInterfaceChanged -= OnStatusChanged;
            _serialService.Error -= OnError;
            _disposed = true;
        }
    }
}
