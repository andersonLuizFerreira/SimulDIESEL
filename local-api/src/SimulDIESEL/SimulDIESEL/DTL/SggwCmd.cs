namespace SimulDIESEL.DTL
{
    /// <summary>
    /// Lista oficial de comandos do protocolo SGGW.
    /// Deve ser idêntica ao firmware.
    /// </summary>
    public enum SggwCmd : byte
    {
        // sistema

        // Comandos GATEWAY Faixa 0x01 - 0x0F
        Ping = 0x55,  //Comando reservado
        GetVersion = 0x01,
        Echo = 0x02,
        LED = 0x03,
        LOGOUT = 0x04
    }
}
