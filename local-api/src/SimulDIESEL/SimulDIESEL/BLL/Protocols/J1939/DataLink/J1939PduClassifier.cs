using SimulDIESEL.BLL.Protocols.J1939.Common;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols.J1939.DataLink
{
    public sealed class J1939PduClassifier
    {
        public bool IsPdu1(J1939IdFieldsDto idFields)
        {
            return idFields != null && idFields.PduFormat < J1939Constants.Pdu1Threshold;
        }

        public bool IsPdu2(J1939IdFieldsDto idFields)
        {
            return idFields != null && idFields.PduFormat >= J1939Constants.Pdu1Threshold;
        }
    }
}
