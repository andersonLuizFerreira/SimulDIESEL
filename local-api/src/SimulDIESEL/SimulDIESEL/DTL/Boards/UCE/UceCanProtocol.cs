namespace SimulDIESEL.DTL.Boards.UCE
{
    public enum UceCanController : byte
    {
        Can0 = 0x00,
        Can1 = 0x01
    }

    public enum UceCanMode : byte
    {
        Normal = 0x00,
        Listen = 0x01
    }

    public enum UceCanInterfaceState : byte
    {
        Disabled = 0x00,
        Configured = 0x01,
        Open = 0x02,
        Fault = 0x03
    }

    public static class UceCanProtocol
    {
        public static bool TryParseController(string rawValue, out UceCanController controller)
        {
            controller = UceCanController.Can0;

            if (string.Equals(rawValue, "can0", System.StringComparison.OrdinalIgnoreCase))
            {
                controller = UceCanController.Can0;
                return true;
            }

            if (string.Equals(rawValue, "can1", System.StringComparison.OrdinalIgnoreCase))
            {
                controller = UceCanController.Can1;
                return true;
            }

            return false;
        }

        public static bool TryParseMode(string rawValue, out UceCanMode mode)
        {
            mode = UceCanMode.Normal;

            if (string.Equals(rawValue, "normal", System.StringComparison.OrdinalIgnoreCase))
            {
                mode = UceCanMode.Normal;
                return true;
            }

            if (string.Equals(rawValue, "listen", System.StringComparison.OrdinalIgnoreCase))
            {
                mode = UceCanMode.Listen;
                return true;
            }

            return false;
        }

        public static bool TryParseBitrateKbps(string rawValue, out int bitrateKbps)
        {
            bitrateKbps = 0;
            if (!int.TryParse(rawValue, System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture, out bitrateKbps))
                return false;

            return TryEncodeBitrate(bitrateKbps, out _);
        }

        public static bool TryEncodeController(UceCanController controller, out byte code)
        {
            switch (controller)
            {
                case UceCanController.Can0:
                    code = 0x00;
                    return true;
                case UceCanController.Can1:
                    code = 0x01;
                    return true;
                default:
                    code = 0x00;
                    return false;
            }
        }

        public static bool TryDecodeController(byte code, out UceCanController controller)
        {
            switch (code)
            {
                case 0x00:
                    controller = UceCanController.Can0;
                    return true;
                case 0x01:
                    controller = UceCanController.Can1;
                    return true;
                default:
                    controller = UceCanController.Can0;
                    return false;
            }
        }

        public static bool TryEncodeMode(UceCanMode mode, out byte code)
        {
            switch (mode)
            {
                case UceCanMode.Normal:
                    code = 0x00;
                    return true;
                case UceCanMode.Listen:
                    code = 0x01;
                    return true;
                default:
                    code = 0x00;
                    return false;
            }
        }

        public static bool TryDecodeMode(byte code, out UceCanMode mode)
        {
            switch (code)
            {
                case 0x00:
                    mode = UceCanMode.Normal;
                    return true;
                case 0x01:
                    mode = UceCanMode.Listen;
                    return true;
                default:
                    mode = UceCanMode.Normal;
                    return false;
            }
        }

        public static bool TryEncodeBitrate(int bitrateKbps, out byte code)
        {
            switch (bitrateKbps)
            {
                case 125:
                    code = 0x00;
                    return true;
                case 250:
                    code = 0x01;
                    return true;
                case 500:
                    code = 0x02;
                    return true;
                case 1000:
                    code = 0x03;
                    return true;
                default:
                    code = 0x00;
                    return false;
            }
        }

        public static bool TryDecodeBitrate(byte code, out int bitrateKbps)
        {
            switch (code)
            {
                case 0x00:
                    bitrateKbps = 125;
                    return true;
                case 0x01:
                    bitrateKbps = 250;
                    return true;
                case 0x02:
                    bitrateKbps = 500;
                    return true;
                case 0x03:
                    bitrateKbps = 1000;
                    return true;
                default:
                    bitrateKbps = 0;
                    return false;
            }
        }

        public static bool TryDecodeState(byte code, out UceCanInterfaceState state)
        {
            switch (code)
            {
                case 0x00:
                    state = UceCanInterfaceState.Disabled;
                    return true;
                case 0x01:
                    state = UceCanInterfaceState.Configured;
                    return true;
                case 0x02:
                    state = UceCanInterfaceState.Open;
                    return true;
                case 0x03:
                    state = UceCanInterfaceState.Fault;
                    return true;
                default:
                    state = UceCanInterfaceState.Disabled;
                    return false;
            }
        }

        public static string ToSdhController(UceCanController controller)
        {
            return controller == UceCanController.Can1 ? "can1" : "can0";
        }

        public static string ToSdhMode(UceCanMode mode)
        {
            return mode == UceCanMode.Listen ? "listen" : "normal";
        }

        public static string ToDisplayMode(UceCanMode mode)
        {
            return mode == UceCanMode.Listen ? "listen" : "normal";
        }

        public static string ToDisplayState(UceCanInterfaceState state)
        {
            switch (state)
            {
                case UceCanInterfaceState.Configured:
                    return "configurada";
                case UceCanInterfaceState.Open:
                    return "aberta";
                case UceCanInterfaceState.Fault:
                    return "falha";
                default:
                    return "desabilitada";
            }
        }

        public static bool IsEnabled(UceCanInterfaceState state)
        {
            return state == UceCanInterfaceState.Open;
        }
    }
}
