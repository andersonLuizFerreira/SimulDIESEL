using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols.J1939.DataLink
{
    public sealed class J1939MessageTypeClassifier
    {
        public J1939MessageTypeDto Classify(J1939IdFieldsDto idFields)
        {
            if (idFields == null)
                return J1939MessageTypeDto.NotJ1939;

            if (idFields.IsIso15765Frame)
                return J1939MessageTypeDto.Iso15765;

            if (idFields.IsReservedEdpDpCombination)
                return J1939MessageTypeDto.Reserved;

            switch (idFields.Pgn)
            {
                case J1939Constants.PgnRequest:
                    return J1939MessageTypeDto.Request;
                case J1939Constants.PgnAcknowledgment:
                    return J1939MessageTypeDto.Acknowledgment;
                case J1939Constants.PgnProprietaryA:
                    return J1939MessageTypeDto.ProprietaryA;
                case J1939Constants.PgnProprietaryA2:
                    return J1939MessageTypeDto.ProprietaryA2;
                case J1939Constants.PgnRequest2:
                    return J1939MessageTypeDto.Request2;
                case J1939Constants.PgnTransfer:
                    return J1939MessageTypeDto.Transfer;
                case J1939Constants.PgnTransportConnectionManagement:
                    return J1939MessageTypeDto.TransportConnectionManagement;
                case J1939Constants.PgnTransportDataTransfer:
                    return J1939MessageTypeDto.TransportDataTransfer;
                default:
                    if (idFields.Pgn >= 0x00FF00 && idFields.Pgn <= 0x00FFFF)
                        return J1939MessageTypeDto.ProprietaryB;

                    return J1939MessageTypeDto.BroadcastOrResponse;
            }
        }
    }
}
