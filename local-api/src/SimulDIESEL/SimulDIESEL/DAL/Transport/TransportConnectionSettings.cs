using System;

namespace SimulDIESEL.DAL.Transport
{
    public abstract class TransportConnectionSettings
    {
        protected TransportConnectionSettings(TransportKind transportKind)
        {
            TransportKind = transportKind;
        }

        public TransportKind TransportKind { get; private set; }
        public string EndpointDisplayName { get; set; }
    }
}
