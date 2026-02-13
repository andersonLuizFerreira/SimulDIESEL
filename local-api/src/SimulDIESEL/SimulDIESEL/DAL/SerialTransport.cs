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
        private SerialPort _port; // acesso protegido por _sync
        private readonly object _sync = new object(); // lock para proteger acesso a _port e _disposed
        private bool _disposed; // acesso protegido por _sync

        public bool IsOpen
        {
            get { lock (_sync) { return _port != null && _port.IsOpen; } }
        }

        public string PortName
        {
            get { lock (_sync) { return _port?.PortName; } }
        }

        public int BaudRate
        {
            get { lock (_sync) { return _port?.BaudRate ?? 0; } }
        }

        /// <summary>Dispara bytes recebidos (crus).</summary>
        public event Action<byte[]> BytesReceived;

        /// <summary>Dispara quando conecta/desconecta.</summary>
        public event Action<bool> ConnectionChanged;

        /// <summary>Dispara erro com mensagem e origem.</summary>
        public event Action<string[]> Error;

        public static string[] ListPorts()
        {
            try
            {
                return SerialPort.GetPortNames();
            }
            catch
            {
                return Array.Empty<string>();
            }
        }

        /// <summary>
        /// Abre a porta serial.
        /// </summary>
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

                if (IsOpen) return true;

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

                    _port.DataReceived += OnDataReceived;
                    _port.ErrorReceived += OnErrorReceived;

                    _port.Open();
                    _port.DiscardInBuffer();
                    _port.DiscardOutBuffer();

                    ConnectionChanged?.Invoke(true);
                    return true;
                }
                catch (Exception ex)
                {
                    SafeClose_NoThrow();
                    RaiseError($"Erro ao conectar: {ex.Message}");
                    return false;
                }
            }
        }

        /// <summary>
        /// Fecha a porta serial.
        /// </summary>
        public void Disconnect()
        {
            lock (_sync)
            {
                if (_disposed) return;

                var wasOpen = IsOpen;
                SafeClose_NoThrow();

                // Só dispara se realmente estava aberto
                if (wasOpen)
                    ConnectionChanged?.Invoke(false);
            }
        }

        /// <summary>
        /// Envia bytes (crus).
        /// </summary>
        public bool Write(byte[] data)
        {
            if (data == null || data.Length == 0) return true;

            lock (_sync)
            {
                ThrowIfDisposed();

                if (!IsOpen)
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
                    // Erro de envio normalmente indica porta inválida/caiu => derruba transporte
                    HandleTransportFault($"Erro ao enviar dados: {ex.Message}");
                    return false;
                }
            }
        }

        private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            // Evento pode vir em thread de sistema.
            try
            {
                SerialPort sp;
                lock (_sync)
                {
                    if (_disposed) return;
                    sp = _port;
                }
                if (sp == null || !sp.IsOpen) return;

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
                // Se deu exception lendo, na prática a porta pode ter caído
                HandleTransportFault($"Erro ao receber dados: {ex.Message}");
            }
        }

        private void OnErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            string msg;

            switch (e.EventType)
            {
                case SerialError.Frame:
                    msg = "Erro de enquadramento na comunicação (Frame).";
                    break;
                case SerialError.Overrun:
                    msg = "Erro de overrun: buffer cheio.";
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

            // Para sua regra (UI trata e usuário reconecta):
            // melhor considerar esses erros como condição de falha do transporte.
            HandleTransportFault(msg);
        }

        private void HandleTransportFault(string message)
        {
            // Garante: fecha e dispara ConnectionChanged(false) uma vez
            bool shouldNotifyDisconnected = false;

            lock (_sync)
            {
                if (_disposed) return;

                if (_port != null && _port.IsOpen)
                    shouldNotifyDisconnected = true;

                SafeClose_NoThrow();
            }

            // Fora do lock para evitar reentrância de UI/eventos
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
                SafeClose_NoThrow();
                _disposed = true;
            }
        }
    }
}
