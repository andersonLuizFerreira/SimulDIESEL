using System;
using SimulDIESEL.DAL.Transport.Bluetooth;
using SimulDIESEL.DAL.Transport.Serial;

namespace SimulDIESEL.DAL.Transport
{
    /// <summary>
    /// Hub de transporte com sessao unica.
    /// Mantem apenas um transporte fisico ativo por vez e expoe um unico
    /// contrato de bytes para a sessao SDGW.
    /// </summary>
    public sealed class SwitchableTransport : IByteTransport
    {
        private readonly object _sync = new object();
        private IByteTransport _activeTransport;
        private bool _disposed;

        public TransportKind SelectedKind { get; private set; } = TransportKind.Serial;
        public string EndpointDisplayName { get; private set; } = "Nenhum";

        public TransportKind Kind
        {
            get { lock (_sync) { return SelectedKind; } }
        }

        public bool IsOpen
        {
            get
            {
                lock (_sync)
                {
                    return _activeTransport != null && _activeTransport.IsOpen;
                }
            }
        }

        public event Action<byte[]> BytesReceived;
        public event Action<bool> ConnectionChanged;
        public event Action<string[]> Error;

        public bool Connect(TransportConnectionSettings settings)
        {
            if (settings == null)
                throw new ArgumentNullException(nameof(settings));

            IByteTransport transport;
            lock (_sync)
            {
                ThrowIfDisposed();

                if (_activeTransport != null && _activeTransport.IsOpen)
                {
                    RaiseError("Ja existe uma sessao ativa. Desconecte o transporte atual antes de iniciar outro.");
                    return false;
                }

                DisposeActiveTransport_NoLock();

                SelectedKind = settings.TransportKind;
                EndpointDisplayName = ResolveDisplayName(settings);
                transport = CreateTransport(settings.TransportKind);
                Subscribe_NoLock(transport);
                _activeTransport = transport;
            }

            bool connected = false;

            try
            {
                connected = transport.Connect(settings);
            }
            catch (Exception ex)
            {
                RaiseError("Falha ao iniciar transporte " + settings.TransportKind + ": " + ex.Message);
                connected = false;
            }

            if (connected)
                return true;

            lock (_sync)
            {
                DisposeActiveTransport_NoLock();
                EndpointDisplayName = "Nenhum";
            }

            return false;
        }

        public bool Write(byte[] data)
        {
            lock (_sync)
            {
                ThrowIfDisposed();

                if (_activeTransport == null)
                {
                    RaiseError("Tentativa de envio sem transporte ativo.");
                    return false;
                }

                return _activeTransport.Write(data);
            }
        }

        public void Disconnect()
        {
            IByteTransport transport;

            lock (_sync)
            {
                if (_disposed)
                    return;

                transport = _activeTransport;
            }

            if (transport == null)
                return;

            transport.Disconnect();
        }

        public void Dispose()
        {
            lock (_sync)
            {
                if (_disposed)
                    return;

                DisposeActiveTransport_NoLock();
                _disposed = true;
            }
        }

        private static IByteTransport CreateTransport(TransportKind kind)
        {
            switch (kind)
            {
                case TransportKind.Serial:
                    return new SerialTransport();
                case TransportKind.Bluetooth:
                    return new BluetoothTransport();
                default:
                    throw new NotSupportedException("Transporte nao suportado: " + kind);
            }
        }

        private static string ResolveDisplayName(TransportConnectionSettings settings)
        {
            if (!string.IsNullOrWhiteSpace(settings.EndpointDisplayName))
                return settings.EndpointDisplayName;

            BluetoothConnectionSettings bluetoothSettings = settings as BluetoothConnectionSettings;
            if (bluetoothSettings != null)
                return bluetoothSettings.GetDisplayName();

            SerialConnectionSettings serialSettings = settings as SerialConnectionSettings;
            if (serialSettings != null)
                return serialSettings.PortName;

            return settings.TransportKind.ToString();
        }

        private void Subscribe_NoLock(IByteTransport transport)
        {
            transport.BytesReceived += OnBytesReceived;
            transport.ConnectionChanged += OnConnectionChanged;
            transport.Error += OnError;
        }

        private void Unsubscribe_NoLock(IByteTransport transport)
        {
            transport.BytesReceived -= OnBytesReceived;
            transport.ConnectionChanged -= OnConnectionChanged;
            transport.Error -= OnError;
        }

        private void DisposeActiveTransport_NoLock()
        {
            if (_activeTransport == null)
                return;

            try
            {
                Unsubscribe_NoLock(_activeTransport);
                _activeTransport.Dispose();
            }
            catch
            {
            }
            finally
            {
                _activeTransport = null;
            }
        }

        private void OnBytesReceived(byte[] data)
        {
            BytesReceived?.Invoke(data);
        }

        private void OnConnectionChanged(bool connected)
        {
            if (!connected)
            {
                lock (_sync)
                {
                    EndpointDisplayName = "Nenhum";
                }
            }

            ConnectionChanged?.Invoke(connected);
        }

        private void OnError(string[] msg)
        {
            Error?.Invoke(msg);
        }

        private void RaiseError(string message)
        {
            Error?.Invoke(new[] { message, "DAL.Transport.SwitchableTransport" });
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(SwitchableTransport));
        }
    }
}
