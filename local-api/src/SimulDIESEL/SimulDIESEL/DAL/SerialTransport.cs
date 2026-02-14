using System;
using System.IO.Ports;

namespace SimulDIESEL.DAL
{
    /// <summary>
    /// DAL: Transporte serial "cru".
    /// Responsabilidades: listar portas, abrir/fechar, enviar bytes, receber bytes (raw).
    /// NÃO faz framing, checksum, CTS, fila, etc.
    /// </summary>
    public sealed class SerialTransport : IDisposable, IByteTransport
    {
        private SerialPort _port;                 // acesso protegido por _sync
        private readonly object _sync = new object();
        private bool _disposed;

        // FONTE DE VERDADE (não confie em SerialPort.IsOpen para "cabo removido")
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

        public bool Connect(string portName, int baudRate,
                            Parity parity = Parity.None,
                            int dataBits = 8,
                            StopBits stopBits = StopBits.One,
                            Handshake handshake = Handshake.None,
                            bool dtrEnable = false,
                            bool rtsEnable = false,
                            int readTimeoutMs = 1000,
                            int writeTimeoutMs = 1000)
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                if (_connected) return true;

                try
                {
                    _port = new SerialPort(portName, baudRate, parity, dataBits, stopBits)
                    {
                        Handshake = handshake,
                        ReadTimeout = readTimeoutMs,
                        WriteTimeout = writeTimeoutMs,
                        DtrEnable = dtrEnable,
                        RtsEnable = rtsEnable
                    };

                    // boas práticas: -= antes de +=
                    _port.DataReceived -= OnDataReceived;
                    _port.ErrorReceived -= OnErrorReceived;

                    _port.DataReceived += OnDataReceived;
                    _port.ErrorReceived += OnErrorReceived;

                    _port.Open();
                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();

                    _connected = true;
                }
                catch (Exception ex)
                {
                    // garante estado consistente
                    SafeClose_NoThrow();
                    _connected = false;

                    RaiseError("Erro ao conectar: " + ex.Message);
                    return false;
                }
            }

            // fora do lock (evita reentrância)
            ConnectionChanged?.Invoke(true);
            return true;
        }

        public void Disconnect()
        {
            bool shouldNotify = false;

            lock (_sync)
            {
                if (_disposed) return;

                // se já está desconectado logicamente, não repete evento
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
            if (data == null || data.Length == 0) return true;

            lock (_sync)
            {
                ThrowIfDisposed();

                if (!_connected || _port == null)
                {
                    // NÃO abra MessageBox em loop; apenas evento + false
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
                    // Falha de escrita = provável cabo removido/porta inválida
                    HandleTransportFault("Erro ao enviar dados: " + ex.Message);
                    return false;
                }
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                SerialPort sp;
                lock (_sync)
                {
                    if (_disposed) return;
                    if (!_connected) return;

                    sp = _port;
                }

                if (sp == null) return;

                int count = sp.BytesToRead;
                if (count <= 0) return;

                var buffer = new byte[count];
                int read = sp.Read(buffer, 0, count);
                if (read <= 0) return;

                if (read != buffer.Length)
                {
                    var trimmed = new byte[read];
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
                case SerialError.Frame: msg = "Erro de enquadramento (Frame)."; break;
                case SerialError.Overrun: msg = "Overrun: buffer cheio."; break;
                case SerialError.RXOver: msg = "Erro RXOver."; break;
                case SerialError.RXParity: msg = "Erro de paridade (RXParity)."; break;
                default: msg = "Erro serial desconhecido."; break;
            }

            HandleTransportFault(msg);
        }

        private void HandleTransportFault(string message)
        {
            bool shouldNotifyDisconnected = false;

            lock (_sync)
            {
                if (_disposed) return;

                if (_connected)
                {
                    shouldNotifyDisconnected = true;
                    _connected = false;
                }

                SafeClose_NoThrow();
            }

            // fora do lock
            RaiseError(message);

            if (shouldNotifyDisconnected)
                ConnectionChanged?.Invoke(false);
        }

        private void RaiseError(string message)
        {
            Error?.Invoke(new[] { message, "DAL.SerialTransport" });
        }

        private void SafeClose_NoThrow()
        {
            try
            {
                if (_port != null)
                {
                    _port.DataReceived -= OnDataReceived;
                    _port.ErrorReceived -= OnErrorReceived;

                    // Close pode lançar quando o cabo some; por isso try/catch externo
                    if (_port.IsOpen)
                        _port.Close();

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
            if (_disposed) throw new ObjectDisposedException(nameof(SerialTransport));
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed) return;

                _connected = false;
                SafeClose_NoThrow();
                _disposed = true;
            }
        }
    }
}
