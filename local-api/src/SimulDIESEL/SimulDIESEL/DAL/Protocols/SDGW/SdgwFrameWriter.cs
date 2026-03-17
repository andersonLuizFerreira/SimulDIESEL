namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public static class SdgwFrameWriter
    {
        public static byte[] BuildStreamFrame(byte cmd, byte flags, byte seq, byte[] payload)
        {
            var raw = new byte[3 + (payload?.Length ?? 0) + 1];
            raw[0] = cmd;
            raw[1] = flags;
            raw[2] = seq;

            if (payload != null && payload.Length > 0)
                System.Buffer.BlockCopy(payload, 0, raw, 3, payload.Length);

            raw[raw.Length - 1] = SdgwFrameCodec.Crc8Atm(raw, 0, raw.Length - 1);

            byte[] encoded = SdgwFrameCodec.CobsEncode(raw);
            var stream = new byte[encoded.Length + 1];
            System.Buffer.BlockCopy(encoded, 0, stream, 0, encoded.Length);
            stream[stream.Length - 1] = 0x00;
            return stream;
        }
    }
}
