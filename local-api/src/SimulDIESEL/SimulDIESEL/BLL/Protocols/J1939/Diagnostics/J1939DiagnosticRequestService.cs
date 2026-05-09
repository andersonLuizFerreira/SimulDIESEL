using System;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Protocols.J1939.Diagnostics
{
    public sealed class J1939DiagnosticRequestService
    {
        public const uint RequestPgn = 59904;
        public const uint Dm1Pgn = 65226;
        public const uint Dm2Pgn = 65227;
        public const byte GlobalDestinationAddress = 0xFF;
        public const byte DefaultToolSourceAddress = 0xF9;

        public CanFrameDto BuildDm1Request()
        {
            return BuildRequest(Dm1Pgn, GlobalDestinationAddress, DefaultToolSourceAddress);
        }

        public CanFrameDto BuildDm2Request()
        {
            return BuildRequest(Dm2Pgn, GlobalDestinationAddress, DefaultToolSourceAddress);
        }

        public CanFrameDto BuildRequest(uint requestedPgn, byte destinationAddress, byte sourceAddress)
        {
            byte[] data = new byte[8];
            data[0] = (byte)(requestedPgn & 0xFF);
            data[1] = (byte)((requestedPgn >> 8) & 0xFF);
            data[2] = (byte)((requestedPgn >> 16) & 0xFF);

            return new CanFrameDto
            {
                CanId = BuildRequestCanId(destinationAddress, sourceAddress),
                IsExtended = true,
                IsRemoteRequest = false,
                Dlc = 3,
                Data = data,
                Timestamp = DateTime.Now,
                Source = CanFrameSource.Unknown
            };
        }

        private static uint BuildRequestCanId(byte destinationAddress, byte sourceAddress)
        {
            return (6U << 26) | (0xEAU << 16) | ((uint)destinationAddress << 8) | sourceAddress;
        }
    }
}
