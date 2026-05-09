using System;
using SimulDIESEL.BLL.Protocols.J1939.Application;
using SimulDIESEL.DTL.Protocols.J1939.Application;
using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;

namespace SimulDIESEL.BLL.Protocols.J1939.Diagnostics
{
    public sealed class J1939DtcParser
    {
        private readonly J1939FmiCatalog _fmiCatalog;
        private readonly J1939PgnCatalog _pgnCatalog;

        public J1939DtcParser()
            : this(new J1939FmiCatalog(), new J1939PgnCatalog())
        {
        }

        public J1939DtcParser(J1939FmiCatalog fmiCatalog, J1939PgnCatalog pgnCatalog)
        {
            _fmiCatalog = fmiCatalog ?? throw new ArgumentNullException(nameof(fmiCatalog));
            _pgnCatalog = pgnCatalog ?? throw new ArgumentNullException(nameof(pgnCatalog));
        }

        public J1939DtcDto Parse(byte b1, byte b2, byte b3, byte b4)
        {
            uint spn = (uint)(b1 | (b2 << 8) | ((b3 & 0xE0) << 11));
            byte fmi = (byte)(b3 & 0x1F);
            byte cm = (byte)((b4 & 0x80) >> 7);
            byte oc = (byte)(b4 & 0x7F);
            J1939FmiDefinitionDto fmiDefinition = _fmiCatalog.Get(fmi);

            return new J1939DtcDto
            {
                Spn = spn,
                SpnName = ResolveSpnName(spn),
                Fmi = fmi,
                FmiDescription = fmiDefinition.Description,
                OccurrenceCount = oc,
                ConversionMethod = cm,
                IsLegacyConversionMethod = cm == 1,
                Status = cm == 1 ? "LegacyConversionMethod" : "CurrentConversionMethod",
                RawBytes = new[] { b1, b2, b3, b4 }
            };
        }

        public bool IsNoDtc(byte b1, byte b2, byte b3, byte b4)
        {
            if (b1 == 0 && b2 == 0 && b3 == 0 && b4 == 0)
                return true;

            J1939DtcDto dtc = Parse(b1, b2, b3, b4);
            return dtc.Spn == 524287U &&
                dtc.Fmi == 31 &&
                dtc.OccurrenceCount == 127 &&
                dtc.ConversionMethod == 1;
        }

        private string ResolveSpnName(uint spn)
        {
            foreach (J1939PgnDefinitionDto pgn in _pgnCatalog.GetAll())
            {
                foreach (J1939SpnDefinitionDto candidate in pgn.Spns)
                {
                    if (candidate.Spn == spn)
                        return candidate.Name;
                }
            }

            return "SPN nao cadastrado";
        }
    }
}
