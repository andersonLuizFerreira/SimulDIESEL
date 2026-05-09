using SimulDIESEL.DTL.Protocols.J1939.Application;

namespace SimulDIESEL.BLL.Protocols.J1939.Application
{
    public sealed class J1939SignalRangeEvaluator
    {
        public J1939SignalStatusDto Evaluate(ulong rawValue, int bitLength)
        {
            if (bitLength == 8)
            {
                if (rawValue == 0xFF) return J1939SignalStatusDto.NotAvailable;
                if (rawValue == 0xFE) return J1939SignalStatusDto.ErrorIndicator;
            }

            if (bitLength == 16)
            {
                if (rawValue == 0xFFFF) return J1939SignalStatusDto.NotAvailable;
                if (rawValue == 0xFFFE) return J1939SignalStatusDto.ErrorIndicator;
            }

            if (bitLength == 32)
            {
                if (rawValue == 0xFFFFFFFF) return J1939SignalStatusDto.NotAvailable;
                if (rawValue == 0xFFFFFFFE) return J1939SignalStatusDto.ErrorIndicator;
            }

            return J1939SignalStatusDto.Valid;
        }

        public J1939SignalStatusDto EvaluateDiscrete2Bit(ulong rawValue)
        {
            switch (rawValue)
            {
                case 0:
                case 1:
                    return J1939SignalStatusDto.Valid;
                case 2:
                    return J1939SignalStatusDto.ErrorIndicator;
                default:
                    return J1939SignalStatusDto.NotAvailable;
            }
        }
    }
}
