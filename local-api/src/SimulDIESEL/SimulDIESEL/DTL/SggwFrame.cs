namespace SimulDIESEL.DTL
{
    /// <summary>
    /// Frame lógico SGGW já decodificado.
    /// Não contém CRC nem COBS (já tratados pelo LinkEngine).
    /// </summary>
    public sealed class SggwFrame
    {
        public byte Cmd { get; }
        public byte Seq { get; }
        public byte Flags { get; }
        public byte[] Payload { get; }

        public SggwFrame(byte cmd, byte seq, byte flags, byte[] payload)
        {
            Cmd = cmd;
            Seq = seq;
            Flags = flags;
            Payload = payload ?? System.Array.Empty<byte>();
        }

        public SggwCmd CommandEnum => (SggwCmd)Cmd;
    }
}
