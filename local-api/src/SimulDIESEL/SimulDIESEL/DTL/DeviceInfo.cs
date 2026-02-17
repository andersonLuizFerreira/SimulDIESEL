namespace SimulDIESEL.DTL
{
    public sealed class DeviceInfo
    {
        public string Version { get; set; }

        public static DeviceInfo FromPayload(byte[] payload)
        {
            return new DeviceInfo
            {
                Version = payload != null
                    ? System.Text.Encoding.ASCII.GetString(payload)
                    : ""
            };
        }
    }
}
