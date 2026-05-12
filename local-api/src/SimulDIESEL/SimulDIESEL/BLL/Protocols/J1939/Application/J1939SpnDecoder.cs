using System.Globalization;
using SimulDIESEL.DTL.Protocols.J1939.Application;

namespace SimulDIESEL.BLL.Protocols.J1939.Application
{
    public sealed class J1939SpnDecoder
    {
        private readonly J1939RawValueReader _rawValueReader;
        private readonly J1939SignalRangeEvaluator _rangeEvaluator;

        public J1939SpnDecoder()
            : this(new J1939RawValueReader(), new J1939SignalRangeEvaluator())
        {
        }

        public J1939SpnDecoder(J1939RawValueReader rawValueReader, J1939SignalRangeEvaluator rangeEvaluator)
        {
            _rawValueReader = rawValueReader;
            _rangeEvaluator = rangeEvaluator;
        }

        public J1939DecodedSignalDto Decode(J1939SpnDefinitionDto definition, byte[] payload)
        {
            J1939DecodedSignalDto signal = new J1939DecodedSignalDto
            {
                Spn = definition.Spn,
                Name = definition.Name,
                Pgn = definition.Pgn,
                Unit = definition.Unit,
                Notes = definition.Notes
            };

            if (definition.PositionPending || !definition.StartByte.HasValue)
            {
                signal.Status = J1939SignalStatusDto.PositionPending;
                signal.DisplayValue = "PositionPending";
                return signal;
            }

            try
            {
                if (definition.IsAscii)
                {
                    string text = _rawValueReader.ReadAscii(payload, definition.StartByte.Value, definition.BitLength / 8);
                    signal.DisplayValue = text;
                    signal.Status = string.IsNullOrEmpty(text) ? J1939SignalStatusDto.NotAvailable : J1939SignalStatusDto.Valid;
                    return signal;
                }

                ulong raw = _rawValueReader.ReadUnsigned(payload, definition.StartByte.Value, definition.StartBit, definition.BitLength);
                signal.RawValue = raw;
                signal.RawHex = J1939RawValueReader.ToRawHex(raw, definition.BitLength);
                signal.Status = definition.IsDiscrete
                    ? _rangeEvaluator.EvaluateDiscrete2Bit(raw)
                    : _rangeEvaluator.Evaluate(raw, definition.BitLength);

                if (signal.Status != J1939SignalStatusDto.Valid)
                {
                    signal.DisplayValue = signal.Status.ToString();
                    return signal;
                }

                if (definition.IsDiscrete)
                {
                    signal.DisplayValue = DecodeDiscrete2Bit(raw);
                    return signal;
                }

                double physical = raw * definition.Resolution + definition.Offset;
                signal.PhysicalValue = physical;
                signal.DisplayValue = physical.ToString("0.###", CultureInfo.InvariantCulture) +
                    (string.IsNullOrWhiteSpace(definition.Unit) ? string.Empty : " " + definition.Unit);
                return signal;
            }
            catch (System.Exception ex)
            {
                signal.Status = J1939SignalStatusDto.DecodeError;
                signal.DisplayValue = "DecodeError";
                signal.Notes = ex.Message;
                return signal;
            }
        }

        private static string DecodeDiscrete2Bit(ulong raw)
        {
            switch (raw)
            {
                case 0:
                    return "Off";
                case 1:
                    return "On";
                case 2:
                    return "ErrorIndicator";
                default:
                    return "NotAvailable";
            }
        }
    }
}
