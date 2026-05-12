namespace SimulDIESEL.DTL.Protocols.J1939.Diagnostics
{
    public sealed class J1939LampStatusDto
    {
        public string Mil { get; set; }
        public string RedStop { get; set; }
        public string AmberWarning { get; set; }
        public string Protect { get; set; }
        public string FlashMil { get; set; }
        public string FlashRedStop { get; set; }
        public string FlashAmberWarning { get; set; }
        public string FlashProtect { get; set; }
        public byte RawLampStatus { get; set; }
        public byte RawFlashStatus { get; set; }
    }
}
