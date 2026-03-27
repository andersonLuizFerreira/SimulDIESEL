using System;

namespace SimulDIESEL.DAL.Transport.Bluetooth
{
    public sealed class BluetoothConnectionSettings : TransportConnectionSettings
    {
        public BluetoothConnectionSettings()
            : base(TransportKind.Bluetooth)
        {
            BaudRate = 115200;
        }

        public string PortName { get; set; }
        public string DeviceName { get; set; }
        public int BaudRate { get; set; }

        public string GetDisplayName()
        {
            if (!string.IsNullOrWhiteSpace(EndpointDisplayName))
                return EndpointDisplayName;

            if (!string.IsNullOrWhiteSpace(DeviceName) && !string.IsNullOrWhiteSpace(PortName))
                return DeviceName + " (" + PortName + ")";

            if (!string.IsNullOrWhiteSpace(DeviceName))
                return DeviceName;

            if (!string.IsNullOrWhiteSpace(PortName))
                return "Bluetooth (" + PortName + ")";

            return "Bluetooth";
        }
    }
}
