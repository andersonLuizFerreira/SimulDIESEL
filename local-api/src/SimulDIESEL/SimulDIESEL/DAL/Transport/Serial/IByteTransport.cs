using System;

namespace SimulDIESEL.DAL.Transport
{
    public interface IByteTransport : IDisposable
    {
        TransportKind Kind { get; }
        bool IsOpen { get; }

        event Action<byte[]> BytesReceived;
        event Action<bool> ConnectionChanged;
        event Action<string[]> Error;

        bool Connect(TransportConnectionSettings settings);
        bool Write(byte[] data);
        void Disconnect();
    }
}
