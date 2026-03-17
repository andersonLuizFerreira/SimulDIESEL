using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.GSA;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.GSA
{
    public static class GsaParsers
    {
        public static bool TryReadBuiltinLedResponse(SggwFrame frame, out GsaLedResponse response, out string error)
        {
            response = null;
            error = null;

            if (frame?.Payload == null)
            {
                error = "Resposta da GSA sem payload para o comando de LED builtin.";
                return false;
            }

            if (frame.Payload.Length != 3 && frame.Payload.Length != 4)
            {
                error = "Resposta da GSA com tamanho inválido para o comando de LED builtin.";
                return false;
            }

            if (frame.Payload[0] != GwProtocol.GsaSetLedType || frame.Payload[1] != 0x01)
            {
                error = "Resposta da GSA com TLV inesperado para o comando de LED builtin.";
                return false;
            }

            if (frame.Payload.Length == 4)
            {
                byte expectedCrc = SdgwFrameCodec.Crc8Atm(frame.Payload, 0, 3);
                if (frame.Payload[3] != expectedCrc)
                {
                    error = "Resposta da GSA com CRC inválido para o comando de LED builtin.";
                    return false;
                }
            }

            response = new GsaLedResponse
            {
                AppliedState = frame.Payload[2] != 0
            };

            return true;
        }
    }
}
