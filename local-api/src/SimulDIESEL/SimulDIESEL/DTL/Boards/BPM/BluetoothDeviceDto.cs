namespace SimulDIESEL.DTL.Boards.BPM
{
    public sealed class BluetoothDeviceDto
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string PortName { get; set; }
        public bool IsPaired { get; set; }
        public bool IsAvailable { get; set; }
        public string StatusText { get; set; }

        public string DisplayName
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Name))
                    return Name;

                if (!string.IsNullOrWhiteSpace(Address))
                    return "Bluetooth " + Address;

                if (!string.IsNullOrWhiteSpace(PortName))
                    return "Bluetooth " + PortName;

                return "Bluetooth";
            }
        }
    }
}
