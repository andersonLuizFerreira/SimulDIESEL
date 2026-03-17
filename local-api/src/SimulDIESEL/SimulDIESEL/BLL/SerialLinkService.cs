using SimulDIESEL.BLL.Boards.BPM.Comm.Serial;

namespace SimulDIESEL.BLL
{
    /// <summary>
    /// Adapter legado e transitório sobre a BPM serial.
    /// A responsabilidade real do link pertence a BLL/Boards/BPM/Comm/Serial.
    /// Este tipo existe apenas para evitar quebra imediata do código que ainda
    /// referencia o ponto global SerialLink/SerialLinkService.
    /// </summary>
    public sealed class SerialLinkService : BpmSerialService
    {
        public new static string[] ListarPortas()
        {
            return BpmSerialService.ListarPortas();
        }
    }
}
