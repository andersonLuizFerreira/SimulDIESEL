using System;

namespace SimulDIESEL.BLL.Boards.GSA
{
    public static class GsaChannelScaling
    {
        private const int MinChannel = 1;
        private const int MaxChannel = 16;
        private const int MaxRaw = 255;
        private const double LowVoltageRangeMax = 5d;
        private const double HighVoltageRangeMax = 12d;
        private const double CurrentRangeMaxMilliamps = 200d;

        public static double GetVoltageRangeMax(int channel)
        {
            ValidateChannel(channel);
            return channel <= 8 ? LowVoltageRangeMax : HighVoltageRangeMax;
        }

        public static double ClampVoltage(int channel, double volts)
        {
            return Clamp(volts, 0d, GetVoltageRangeMax(channel));
        }

        public static byte VoltsToRaw(int channel, double volts)
        {
            double maxVoltage = GetVoltageRangeMax(channel);
            double clampedVolts = Clamp(volts, 0d, maxVoltage);
            int raw = (int)Math.Round((clampedVolts / maxVoltage) * MaxRaw, MidpointRounding.AwayFromZero);
            return (byte)Clamp(raw, 0, MaxRaw);
        }

        public static double RawToVolts(int channel, byte raw)
        {
            double maxVoltage = GetVoltageRangeMax(channel);
            return (raw / (double)MaxRaw) * maxVoltage;
        }

        public static double RawToMilliamps(byte raw)
        {
            return (raw / (double)MaxRaw) * CurrentRangeMaxMilliamps;
        }

        private static void ValidateChannel(int channel)
        {
            if (channel < MinChannel || channel > MaxChannel)
                throw new ArgumentOutOfRangeException(nameof(channel), "Canal da GSA deve estar entre 1 e 16.");
        }

        private static double Clamp(double value, double minimum, double maximum)
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
        }

        private static int Clamp(int value, int minimum, int maximum)
        {
            if (value < minimum)
                return minimum;

            if (value > maximum)
                return maximum;

            return value;
        }
    }
}
