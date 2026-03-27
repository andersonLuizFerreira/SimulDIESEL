namespace SimulDIESEL.BLL.Boards.BPM.Comm.Bluetooth
{
    public sealed class BpmBluetoothService
    {
        private readonly Serial.BpmSerialService _service;

        internal BpmBluetoothService(Serial.BpmSerialService service)
        {
            _service = service;
        }

        public string[] ListarPortas()
        {
            return Serial.BpmSerialService.ListarBluetoothPortas();
        }

        public bool Connect(string portName, string deviceName, int baudRate = 115200)
        {
            return _service.ConnectBluetooth(portName, deviceName, baudRate);
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
