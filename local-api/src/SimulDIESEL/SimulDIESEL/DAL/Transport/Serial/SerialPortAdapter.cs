using System;
using System.IO.Ports;

namespace SimulDIESEL.DAL.Transport.Serial
{
    public sealed class SerialPortAdapter : IDisposable
    {
        private readonly SerialPort _serialPort;

        public SerialPortAdapter(SerialConnectionSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            _serialPort = new SerialPort(
                settings.PortName,
                settings.BaudRate,
                settings.Parity,
                settings.DataBits,
                settings.StopBits)
            {
                Handshake = settings.Handshake,
                ReadTimeout = settings.ReadTimeoutMs,
                WriteTimeout = settings.WriteTimeoutMs,
                DtrEnable = settings.DtrEnable,
                RtsEnable = settings.RtsEnable
            };
        }

        public event SerialDataReceivedEventHandler DataReceived
        {
            add { _serialPort.DataReceived += value; }
            remove { _serialPort.DataReceived -= value; }
        }

        public event SerialErrorReceivedEventHandler ErrorReceived
        {
            add { _serialPort.ErrorReceived += value; }
            remove { _serialPort.ErrorReceived -= value; }
        }

        public bool IsOpen => _serialPort.IsOpen;
        public string PortName => _serialPort.PortName;
        public int BaudRate => _serialPort.BaudRate;
        public int BytesToRead => _serialPort.BytesToRead;

        public void Open()
        {
            _serialPort.Open();
        }

        public int Read(byte[] buffer, int offset, int count)
        {
            return _serialPort.Read(buffer, offset, count);
        }

        public void Write(byte[] buffer, int offset, int count)
        {
            _serialPort.Write(buffer, offset, count);
        }

        public void DiscardInBuffer()
        {
            _serialPort.DiscardInBuffer();
        }

        public void DiscardOutBuffer()
        {
            _serialPort.DiscardOutBuffer();
        }

        public void Dispose()
        {
            try
            {
                if (_serialPort.IsOpen)
                    _serialPort.Close();
            }
            catch
            {
            }

            _serialPort.Dispose();
        }
    }
}
