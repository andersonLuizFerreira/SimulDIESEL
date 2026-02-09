using System;
using System.IO.Ports;
using SimulDIESEL.DAL;

namespace SimulDIESEL.BLL
{
    /// <summary>
    /// BLL mínima para conexão serial (sem tráfego/protocolo).
    /// UI fala com esta classe. Ela usa a DAL (SerialTransport).
    /// </summary>
    public sealed class SerialLinkService : IDisposable
    {
        private readonly SerialTransport _transport; // acesso protegido por _disposed
        private bool _disposed; // acesso protegido por _disposed


        public SerialLinkService()
        {
            _transport = new SerialTransport();

            _transport.ConnectionChanged -= OnConnectionChanged; // para evitar múltiplas inscrições se criar mais de um SerialLinkService (não deveria, mas só pra garantir)
            _transport.ConnectionChanged += OnConnectionChanged; // repassa para UI

            _transport.Error -= OnError; // para evitar múltiplas inscrições se criar mais de um SerialLinkService (não deveria, mas só pra garantir)
            _transport.Error += OnError; // repassa para UI

            _transport.BytesReceived -= OnBytesReceived; // para evitar múltiplas inscrições se criar mais de um SerialLinkService (não deveria, mas só pra garantir)
            _transport.BytesReceived += OnBytesReceived; // por enquanto, só repassa (ou nem usa)
        }

        public bool IsConnected => _transport.IsOpen; // repassa para UI

        public static string[] ListarPortas() // repassa para UI
        {
            return SerialTransport.ListPorts();
        }

        public event Action<bool> ConnectionChanged;
        public event Action<string[]> Error;
        public event Action<byte[]> BytesReceived; // opcional (debug), não vamos usar tráfego ainda

        public bool Connect(string portName, int baudRate,
                            bool dtrEnable = false,
                            bool rtsEnable = false,
                            Parity parity = Parity.None,
                            int dataBits = 8,
                            StopBits stopBits = StopBits.One,
                            Handshake handshake = Handshake.None)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(SerialLinkService));

            return _transport.Connect(
                portName: portName,
                baudRate: baudRate,
                parity: parity,
                dataBits: dataBits,
                stopBits: stopBits,
                handshake: handshake,
                dtrEnable: dtrEnable,
                rtsEnable: rtsEnable
            );
        }

        public void Disconnect()
        {
            if (_disposed) return;
            _transport.Disconnect();
        }

        private void OnConnectionChanged(bool connected)
        {
            ConnectionChanged?.Invoke(connected);
            Console.WriteLine($" SerialLinkService. ConnectionChanged Invoked. Connected={connected}");
        }

        private void OnError(string[] msg)
        {
            Error?.Invoke(msg);
        }

        private void OnBytesReceived(byte[] data)
        {
            // Sem protocolo ainda. Deixo repassado só para debug.
            BytesReceived?.Invoke(data);
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            _transport.ConnectionChanged -= OnConnectionChanged;
            _transport.Error -= OnError;
            _transport.BytesReceived -= OnBytesReceived;

            _transport.Dispose();
        }
    }
}
