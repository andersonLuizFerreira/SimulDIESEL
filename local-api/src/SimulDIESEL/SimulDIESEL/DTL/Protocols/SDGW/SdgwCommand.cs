namespace SimulDIESEL.DTL.Protocols.SDGW
{
    /// <summary>
    /// Comandos reservados do enlace SDGW.
    /// Os comandos compactos do gateway são resolvidos em tempo de execução pelo mapeador SDH.
    /// </summary>
    public enum SdgwCommand : byte
    {
        Ping = 0x55,
        Ack = 0xF1,
        Err = 0xF2
    }
}
