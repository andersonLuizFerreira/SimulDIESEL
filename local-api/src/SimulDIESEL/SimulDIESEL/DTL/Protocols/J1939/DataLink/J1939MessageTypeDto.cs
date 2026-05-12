namespace SimulDIESEL.DTL.Protocols.J1939.DataLink
{
    public enum J1939MessageTypeDto
    {
        Unknown = 0,
        Command = 1,
        Request = 2,
        BroadcastOrResponse = 3,
        Acknowledgment = 4,
        GroupFunction = 5,
        Request2 = 6,
        Transfer = 7,
        TransportConnectionManagement = 8,
        TransportDataTransfer = 9,
        ProprietaryA = 10,
        ProprietaryA2 = 11,
        ProprietaryB = 12,
        Iso15765 = 13,
        Reserved = 14,
        NotJ1939 = 15
    }
}
