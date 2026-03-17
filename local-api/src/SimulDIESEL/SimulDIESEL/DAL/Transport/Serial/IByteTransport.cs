using System;

namespace SimulDIESEL.DAL.Transport.Serial
{
    public interface IByteTransport : IDisposable
    {
        bool IsOpen { get; }

        event Action<byte[]> BytesReceived;
        event Action<bool> ConnectionChanged;
        event Action<string[]> Error;

        bool Write(byte[] data);
        void Disconnect();
    }
}
