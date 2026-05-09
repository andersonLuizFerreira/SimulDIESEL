namespace SimulDIESEL.DTL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939NameDto
    {
        public uint IdentityNumber { get; set; }
        public ushort ManufacturerCode { get; set; }
        public byte EcuInstance { get; set; }
        public byte FunctionInstance { get; set; }
        public byte Function { get; set; }
        public byte Reserved { get; set; }
        public byte VehicleSystem { get; set; }
        public byte VehicleSystemInstance { get; set; }
        public byte IndustryGroup { get; set; }
        public bool IsArbitraryAddressCapable { get; set; }
        public byte[] RawNameBytes { get; set; }
        public ulong RawNameUInt64 { get; set; }
        public string NameHex { get; set; }
    }
}
