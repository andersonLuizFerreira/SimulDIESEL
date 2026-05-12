using SimulDIESEL.DTL.Protocols.J1939.Diagnostics;

namespace SimulDIESEL.BLL.Protocols.J1939.Diagnostics
{
    public sealed class J1939LampStatusDecoder
    {
        public J1939LampStatusDto Decode(byte lampStatus, byte flashStatus)
        {
            return new J1939LampStatusDto
            {
                RawLampStatus = lampStatus,
                RawFlashStatus = flashStatus,
                Protect = DecodeLamp(lampStatus & 0x03),
                AmberWarning = DecodeLamp((lampStatus >> 2) & 0x03),
                RedStop = DecodeLamp((lampStatus >> 4) & 0x03),
                Mil = DecodeLamp((lampStatus >> 6) & 0x03),
                FlashProtect = DecodeFlash(flashStatus & 0x03),
                FlashAmberWarning = DecodeFlash((flashStatus >> 2) & 0x03),
                FlashRedStop = DecodeFlash((flashStatus >> 4) & 0x03),
                FlashMil = DecodeFlash((flashStatus >> 6) & 0x03)
            };
        }

        private static string DecodeLamp(int value)
        {
            switch (value)
            {
                case 0:
                    return "Off";
                case 1:
                    return "On";
                case 2:
                    return "Reserved";
                default:
                    return "NotAvailable";
            }
        }

        private static string DecodeFlash(int value)
        {
            switch (value)
            {
                case 0:
                    return "SlowFlash";
                case 1:
                    return "FastFlash";
                case 2:
                    return "Reserved";
                default:
                    return "DoNotFlashOrNotAvailable";
            }
        }
    }
}
