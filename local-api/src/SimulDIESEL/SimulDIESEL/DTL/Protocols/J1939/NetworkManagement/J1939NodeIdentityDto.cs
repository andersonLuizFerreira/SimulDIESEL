using System;

namespace SimulDIESEL.DTL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939NodeIdentityDto
    {
        public int SourceAddressDecimal { get; set; }
        public string SourceAddressHex { get; set; }
        public string NameHex { get; set; }
        public int ManufacturerCode { get; set; }
        public string ManufacturerName { get; set; }
        public bool ManufacturerKnown { get; set; }
        public int FunctionCode { get; set; }
        public string FunctionName { get; set; }
        public bool FunctionKnown { get; set; }
        public int IndustryGroupCode { get; set; }
        public string IndustryGroupName { get; set; }
        public bool IndustryGroupKnown { get; set; }
        public int VehicleSystemCode { get; set; }
        public string VehicleSystemName { get; set; }
        public bool VehicleSystemKnown { get; set; }
        public string PreferredAddressName { get; set; }
        public bool PreferredAddressKnown { get; set; }
        public int EcuInstance { get; set; }
        public int FunctionInstance { get; set; }
        public int VehicleSystemInstance { get; set; }
        public uint IdentityNumber { get; set; }
        public bool ArbitraryAddressCapable { get; set; }
        public DateTime LastSeenAt { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
    }
}
