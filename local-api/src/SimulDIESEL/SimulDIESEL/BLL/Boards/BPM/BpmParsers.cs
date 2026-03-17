using SimulDIESEL.DTL.Common;

namespace SimulDIESEL.BLL.Boards.BPM
{
    public static class BpmParsers
    {
        private const string InterfacePrefix = "SimulDIESEL ver";

        public static bool TryParseInterfaceInfo(string line, out DeviceInfo deviceInfo)
        {
            deviceInfo = null;

            if (string.IsNullOrWhiteSpace(line) || line.IndexOf(InterfacePrefix, System.StringComparison.OrdinalIgnoreCase) < 0)
                return false;

            deviceInfo = new DeviceInfo { Version = line.Trim() };
            return true;
        }
    }
}
