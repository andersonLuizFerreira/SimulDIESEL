using System;
using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939AddressClaimRequestService
    {
        public CanFrameDto BuildGlobalRequest()
        {
            return BuildRequest(J1939ToolAddressConfig.GlobalAddress, J1939ToolAddressConfig.DefaultToolSourceAddress);
        }

        public CanFrameDto BuildRequest(byte destinationAddress, byte sourceAddress)
        {
            byte[] data = new byte[8];
            data[0] = 0x00;
            data[1] = 0xEE;
            data[2] = 0x00;

            return new CanFrameDto
            {
                CanId = (6U << 26) | (J1939Constants.PgnRequest << 8) | ((uint)destinationAddress << 8) | sourceAddress,
                IsExtended = true,
                IsRemoteRequest = false,
                Dlc = 3,
                Data = data,
                Timestamp = DateTime.Now,
                Source = CanFrameSource.Unknown
            };
        }
    }
}
