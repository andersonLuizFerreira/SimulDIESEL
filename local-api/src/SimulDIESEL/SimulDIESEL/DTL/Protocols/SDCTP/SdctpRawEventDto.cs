using System;

namespace SimulDIESEL.DTL.Protocols.SDCTP
{
    public sealed class SdctpRawEventDto
    {
        public byte Type { get; set; }
        public byte[] Payload { get; set; }
        public DateTime TimestampUtc { get; set; }

        public SdctpRawEventDto Clone()
        {
            return new SdctpRawEventDto
            {
                Type = Type,
                Payload = Payload != null ? (byte[])Payload.Clone() : new byte[0],
                TimestampUtc = TimestampUtc
            };
        }
    }
}
