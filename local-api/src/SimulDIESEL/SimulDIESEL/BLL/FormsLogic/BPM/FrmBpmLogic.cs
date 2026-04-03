using System;
using System.IO.Ports;
using SimulDIESEL.BLL.Boards.BPM;
using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;
using SimulDIESEL.DAL.Transport;
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

        public string[] ListarBluetoothPortas()
        {
            return BpmSerialService.ListarBluetoothPortas();
        }

        public BluetoothDeviceDto[] ListarBluetoothDispositivos()
        {
            return _serialService.Bluetooth.ListarDispositivos();
        }

        public BpmCommandResult ConnectBluetoothPadrao(int baudRate = 115200)
        {
            return _serialService.Bluetooth.ConnectDefault(baudRate);
        }

        public BpmStatusDto GetStatus()
        {
            return _serialService.Bpm.GetStatus();
        }

        public string GetInterfaceDisplayName()
        {
            BpmStatusDto status = GetStatus();

            if (!status.IsConnected)
                return "Nenhum";

            if (status.TransportKind == TransportKind.Bluetooth)
            {
                string bluetoothName = !string.IsNullOrWhiteSpace(status.TransportDisplayName)
                    ? status.TransportDisplayName
                    : status.InterfaceName;
                return "Bluetooth - " + bluetoothName;
            }

            if (!string.IsNullOrWhiteSpace(status.TransportDisplayName))
                return status.TransportDisplayName;

            if (!string.IsNullOrWhiteSpace(status.InterfaceName))
                return status.InterfaceName;

            return "Nenhum";
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

        public bool ConnectBluetooth(string portName, string deviceName, int baudRate = 115200)
        {
            if (string.IsNullOrWhiteSpace(portName))
                throw new ArgumentException("Porta COM Bluetooth e obrigatoria.", nameof(portName));

            return _serialService.ConnectBluetooth(portName, deviceName, baudRate);
        }

        public bool ConnectBluetooth(BluetoothDeviceDto device, int baudRate = 115200)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));

            if (string.IsNullOrWhiteSpace(device.PortName))
                throw new ArgumentException("O dispositivo selecionado nao possui porta SPP utilizavel no host.", nameof(device));

            return _serialService.Bluetooth.Connect(device, baudRate);
        }

        public TransportKind GetSelectedTransportKind()
        {
            return _serialService.SelectedTransportKind;
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
