using SimulDIESEL.DAL.Transport.Bluetooth;
using SimulDIESEL.DTL.Boards.BPM;

namespace SimulDIESEL.BLL.Boards.BPM.Comm.Bluetooth
{
    public sealed class BpmBluetoothService
    {
        private readonly Serial.BpmSerialService _service;

        internal BpmBluetoothService(Serial.BpmSerialService service)
        {
            _service = service;
        }

        public BluetoothDeviceDto[] ListarDispositivos()
        {
            return BluetoothDeviceCatalog.ListDevices();
        }

        public BpmCommandResult ConnectDefault(int baudRate = 115200)
        {
            if (_service.IsConnected && !_service.IsBluetoothOpen)
            {
                return BpmCommandResult.Fail(
                    "Desconecte a Serial antes de conectar via Bluetooth.");
            }

            BluetoothDeviceDto device;
            string reason;
            if (!BluetoothDeviceCatalog.TryResolvePreferredBpmDevice(out device, out reason))
                return BpmCommandResult.Fail(reason);

            bool connected = Connect(device, baudRate);
            if (!connected)
            {
                return BpmCommandResult.Fail(
                    "Nao foi possivel conectar ao Bluetooth da BPM pela porta " + device.PortName + ".");
            }

            return BpmCommandResult.Succeeded(
                "Bluetooth conectado em " + device.DisplayName + " (" + device.PortName + ").");
        }

        public bool Connect(BluetoothDeviceDto device, int baudRate = 115200)
        {
            if (device == null)
                return false;

            return _service.ConnectBluetooth(device.PortName, device.DisplayName, baudRate);
        }

        public void Disconnect()
        {
            _service.Disconnect();
        }

        public BpmCommandResult GetStatus()
        {
            if (_service.IsBluetoothOpen)
                return BpmCommandResult.Succeeded("Transporte Bluetooth da BPM conectado.");

            if (_service.IsConnected)
                return BpmCommandResult.Fail("Existe outra sessao ativa na BPM. Desconecte o transporte atual antes de usar Bluetooth.");

            return BpmCommandResult.Fail("Transporte Bluetooth da BPM desconectado.");
        }
    }
}
