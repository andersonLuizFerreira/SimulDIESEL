namespace SimulDIESEL.BLL.Boards.BPM.Backplane
{
    public sealed class BackplaneService
    {
        public BpmCommandResult GetStatus()
        {
            return BpmCommandResult.Succeeded("Monitoramento de backplane da BPM preparado para expansão.");
        }
    }
}
