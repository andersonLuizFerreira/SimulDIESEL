namespace SimulDIESEL.BLL.Boards.BPM.XConn
{
    public sealed class XConnService
    {
        public BpmCommandResult GetStatus()
        {
            return BpmCommandResult.Succeeded("Detecção de X-Conn da BPM preparada para expansão.");
        }
    }
}
