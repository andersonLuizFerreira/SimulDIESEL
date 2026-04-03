namespace SimulDIESEL.DTL.Boards.GSA
{
    public enum GsaOffsetKind : byte
    {
        Vout = 0x01,
        Vread = 0x02,
        Iread = 0x03
    }

    public enum GsaErrorCode : byte
    {
        InvalidChannel = 0x01,
        InvalidValue = 0x02,
        InvalidState = 0x03,
        InvalidKind = 0x04,
        FaultLatched = 0x05,
        EepromWriteFailed = 0x06,
        CommandNotSupported = 0x07,
        InvalidPayload = 0x08,
        InvalidTlvCrc = 0x09,
        PhysicalFaultStillPresent = 0x0A,
        OperationNotAllowedInCurrentState = 0x0B
    }

    public enum GsaPhysicalOperationStatus : byte
    {
        Ok = 0x01,
        TcaNoAck = 0x02,
        McpNoAck = 0x03
    }

    public class GsaChannelSnapshot
    {
        public int Channel { get; set; }
        public byte Setpoint { get; set; }
        public byte VoltageRead { get; set; }
        public byte CurrentRead { get; set; }
        public bool Enabled { get; set; }
        public bool Fault { get; set; }
    }

    public sealed class GsaChannelFaultEvent : GsaChannelSnapshot
    {
    }

    public sealed class GsaPhysicalOperationEvent
    {
        public byte OriginType { get; set; }
        public int Channel { get; set; }
        public GsaPhysicalOperationStatus Status { get; set; }
        public string Message { get; set; }

        public bool IsSuccess
        {
            get { return Status == GsaPhysicalOperationStatus.Ok; }
        }
    }

    public sealed class GsaGatewayErrorResponse
    {
        public byte ErrorCode { get; set; }
        public string Message { get; set; }
    }

    public sealed class GsaFunctionalErrorResponse
    {
        public byte RequestType { get; set; }
        public int Channel { get; set; }
        public GsaErrorCode ErrorCode { get; set; }
        public string Message { get; set; }
    }
}
