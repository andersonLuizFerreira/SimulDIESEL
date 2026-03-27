using System;
using SimulDIESEL.DAL.Transport.Serial;

namespace SimulDIESEL.DAL.Transport.Bluetooth
{
    /// <summary>
    /// Transporte Bluetooth Classic SPP no host Windows.
    /// Nesta primeira versão o canal BT é exposto como porta COM pareada.
    /// </summary>
    public sealed class BluetoothTransport : IByteTransport
    {
        private readonly SerialTransport _serialTransport = new SerialTransport();

        public BluetoothTransport()
        {
            _serialTransport.BytesReceived += OnBytesReceived;
            _serialTransport.ConnectionChanged += OnConnectionChanged;
            _serialTransport.Error += OnError;
        }

        public TransportKind Kind => TransportKind.Bluetooth;
        public bool IsOpen => _serialTransport.IsOpen;
        public string PortName => _serialTransport.PortName;
        public int BaudRate => _serialTransport.BaudRate;

        public event Action<byte[]> BytesReceived;
        public event Action<bool> ConnectionChanged;
        public event Action<string[]> Error;

        public static string[] ListPorts()
        {
            return SerialTransport.ListPorts();
        }

        public bool Connect(TransportConnectionSettings settings)
        {
            BluetoothConnectionSettings bluetoothSettings = settings as BluetoothConnectionSettings;
            if (bluetoothSettings == null)
                throw new ArgumentException("Configuracao invalida para transporte Bluetooth.", nameof(settings));

            return Connect(bluetoothSettings);
        }

        public bool Connect(BluetoothConnectionSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            return _serialTransport.Connect(
                portName: settings.PortName,
                baudRate: settings.BaudRate);
        }

        public bool Write(byte[] data)
        {
            return _serialTransport.Write(data);
        }

        public void Disconnect()
        {
            _serialTransport.Disconnect();
        }

        public void Dispose()
        {
            _serialTransport.BytesReceived -= OnBytesReceived;
            _serialTransport.ConnectionChanged -= OnConnectionChanged;
            _serialTransport.Error -= OnError;
            _serialTransport.Dispose();
        }

        private void OnBytesReceived(byte[] data)
        {
            BytesReceived?.Invoke(data);
        }

        private void OnConnectionChanged(bool connected)
        {
            ConnectionChanged?.Invoke(connected);
        }

        private void OnError(string[] msg)
        {
            Error?.Invoke(msg);
        }
    }
}
