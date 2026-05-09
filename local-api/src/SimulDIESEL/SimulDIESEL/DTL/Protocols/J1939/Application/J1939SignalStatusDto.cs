namespace SimulDIESEL.DTL.Protocols.J1939.Application
{
    public enum J1939SignalStatusDto
    {
        Valid = 0,
        ParameterSpecificIndicator = 1,
        Reserved = 2,
        ErrorIndicator = 3,
        NotAvailable = 4,
        NotRequested = 5,
        PositionPending = 6,
        UnsupportedPgn = 7,
        DecodeError = 8
    }
}
