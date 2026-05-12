namespace SimulDIESEL.DTL.Protocols.J1939.Diagnostics
{
    public sealed class J1939DiagnosticReadResultDto
    {
        public bool Dm1RequestSent { get; set; }
        public bool Dm2RequestSent { get; set; }
        public string Status { get; set; }
    }
}
