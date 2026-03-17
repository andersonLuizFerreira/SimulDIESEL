namespace SimulDIESEL.DTL.Protocols.SDGW
{
    /// <summary>
    /// Lista oficial de comandos do protocolo SGGW.
    /// Deve ser idêntica ao firmware.
    /// </summary>
    public enum SggwCmd : byte
    {
        Ping = 0x55,
        GetVersion = 0x01,
        Echo = 0x02,
        LED = 0x03,
        LOGOUT = 0x04
    }
}
