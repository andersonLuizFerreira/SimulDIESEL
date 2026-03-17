using System;
using System.IO.Ports;

namespace SimulDIESEL.DAL.Transport.Serial
{
    /// <summary>
    /// Transporte serial cru. Não conhece framing, protocolo ou regra funcional.
    /// </summary>
    public sealed class SerialTransport : IDisposable, IByteTransport
    {
        private SerialPortAdapter _port;
        private readonly object _sync = new object();
        private bool _disposed;
        private bool _connected;

        public bool IsOpen
        {
            get { lock (_sync) { return _connected; } }
        }

        public string PortName
        {
            get { lock (_sync) { return _port != null ? _port.PortName : null; } }
        }

        public int BaudRate
        {
            get { lock (_sync) { return _port != null ? _port.BaudRate : 0; } }
        }

        public event Action<byte[]> BytesReceived;
        public event Action<bool> ConnectionChanged;
        public event Action<string[]> Error;

        public static string[] ListPorts()
        {
            try { return SerialPort.GetPortNames(); }
            catch { return Array.Empty<string>(); }
        }

        public bool Connect(SerialConnectionSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            lock (_sync)
            {
                ThrowIfDisposed();

                if (_connected)
                    return true;

                try
                {
                    _port = new SerialPortAdapter(settings);
                    _port.DataReceived += OnDataReceived;
                    _port.ErrorReceived += OnErrorReceived;
                    _port.Open();
                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();
                    _connected = true;
                }
                catch (Exception ex)
                {
                    SafeClose_NoThrow();
                    _connected = false;
                    RaiseError("Erro ao conectar: " + ex.Message);
                    return false;
                }
            }

            ConnectionChanged?.Invoke(true);
            return true;
        }

        public bool Connect(
            string portName,
            int baudRate,
            Parity parity = Parity.None,
            int dataBits = 8,
            StopBits stopBits = StopBits.One,
            Handshake handshake = Handshake.None,
            bool dtrEnable = false,
            bool rtsEnable = false,
            int readTimeoutMs = 1000,
            int writeTimeoutMs = 1000)
        {
            return Connect(new SerialConnectionSettings
            {
                PortName = portName,
                BaudRate = baudRate,
                Parity = parity,
                DataBits = dataBits,
                StopBits = stopBits,
                Handshake = handshake,
                DtrEnable = dtrEnable,
                RtsEnable = rtsEnable,
                ReadTimeoutMs = readTimeoutMs,
                WriteTimeoutMs = writeTimeoutMs
            });
        }

        public void Disconnect()
        {
            bool shouldNotify = false;

            lock (_sync)
            {
                if (_disposed)
                    return;

                if (_connected)
                {
                    shouldNotify = true;
                    _connected = false;
                }

                SafeClose_NoThrow();
            }

            if (shouldNotify)
                ConnectionChanged?.Invoke(false);
        }

        public bool Write(byte[] data)
        {
            if (data == null || data.Length == 0)
                return true;

            lock (_sync)
            {
                ThrowIfDisposed();

                if (!_connected || _port == null)
                {
                    RaiseError("Tentativa de envio com porta fechada.");
                    return false;
                }

                try
                {
                    _port.Write(data, 0, data.Length);
                    return true;
                }
                catch (Exception ex)
                {
                    HandleTransportFault("Erro ao enviar dados: " + ex.Message);
                    return false;
                }
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPortAdapter serialPort;
                lock (_sync)
                {
                    if (_disposed || !_connected)
                        return;

                    serialPort = _port;
                }

                if (serialPort == null)
                    return;

                int count = serialPort.BytesToRead;
                if (count <= 0)
                    return;

                byte[] buffer = new byte[count];
                int read = serialPort.Read(buffer, 0, count);
                if (read <= 0)
                    return;

                if (read != buffer.Length)
                {
                    byte[] trimmed = new byte[read];
                    Buffer.BlockCopy(buffer, 0, trimmed, 0, read);
                    buffer = trimmed;
                }

                BytesReceived?.Invoke(buffer);
            }
            catch (Exception ex)
            {
                HandleTransportFault("Erro ao receber dados: " + ex.Message);
            }
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            string msg;
            switch (e.EventType)
            {
                case SerialError.Frame:
                    msg = "Erro de enquadramento (Frame).";
                    break;
                case SerialError.Overrun:
                    msg = "Overrun: buffer cheio.";
                    break;
                case SerialError.RXOver:
                    msg = "Erro RXOver.";
                    break;
                case SerialError.RXParity:
                    msg = "Erro de paridade (RXParity).";
                    break;
                default:
                    msg = "Erro serial desconhecido.";
                    break;
            }

            HandleTransportFault(msg);
        }

        private void HandleTransportFault(string message)
        {
            bool shouldNotifyDisconnected = false;

            lock (_sync)
            {
                if (_disposed)
                    return;

                if (_connected)
                {
                    shouldNotifyDisconnected = true;
                    _connected = false;
                }

                SafeClose_NoThrow();
            }

            RaiseError(message);

            if (shouldNotifyDisconnected)
                ConnectionChanged?.Invoke(false);
        }

        private void RaiseError(string message)
        {
            Error?.Invoke(new[] { message, "DAL.Transport.Serial.SerialTransport" });
        }

        private void SafeClose_NoThrow()
        {
            try
            {
                if (_port != null)
                {
                    _port.DataReceived -= OnDataReceived;
                    _port.ErrorReceived -= OnErrorReceived;
                    _port.Dispose();
                    _port = null;
                }
            }
            catch
            {
                _port = null;
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SerialTransport));
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                _connected = false;
                SafeClose_NoThrow();
                _disposed = true;
            }
        }
    }
}
