using System;
using System.Threading.Tasks;
using SimulDIESEL.DAL.Protocols.SDGW;
using SimulDIESEL.DTL.Boards.BPM;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Boards.BPM
{
    public sealed class BpmClient
    {
        private readonly SdhClient _sdhClient;
        private readonly Comm.Serial.BpmSerialService _serialService;
        private readonly Backplane.BackplaneService _backplaneService;
        private readonly XConn.XConnService _xConnService;

        public BpmClient(
            SdhClient sdhClient,
            Comm.Serial.BpmSerialService serialService,
            Backplane.BackplaneService backplaneService,
            XConn.XConnService xConnService)
        {
            _sdhClient = sdhClient ?? throw new ArgumentNullException(nameof(sdhClient));
            _serialService = serialService ?? throw new ArgumentNullException(nameof(serialService));
            _backplaneService = backplaneService ?? throw new ArgumentNullException(nameof(backplaneService));
            _xConnService = xConnService ?? throw new ArgumentNullException(nameof(xConnService));
        }

        public Backplane.BackplaneService Backplane => _backplaneService;
        public XConn.XConnService XConn => _xConnService;

        public BpmStatusDto GetStatus()
        {
            return new BpmStatusDto
            {
                IsConnected = _serialService.IsConnected,
                IsLinked = _serialService.IsLinked,
                InterfaceName = _serialService.NomeDaInterface,
                LinkState = _serialService.State.ToString()
            };
        }

        public async Task<BpmCommandResult> PingGatewayAsync()
        {
            var command = new SdhCommand
            {
                Target = "BPM.gateway",
                Op = "ping"
            };

            SdGwLinkEngine.SendOutcome outcome = await _sdhClient
                .SendAsync(command, SdGwTxPriority.High, "BPM gateway ping")
                .ConfigureAwait(false);
            switch (outcome)
            {
                case SdGwLinkEngine.SendOutcome.Acked:
                    return BpmCommandResult.Succeeded("Ping da BPM concluído com ACK.", outcome);
                case SdGwLinkEngine.SendOutcome.TransportDown:
                    return BpmCommandResult.Fail("O transporte serial da BPM está indisponível.", outcome);
                case SdGwLinkEngine.SendOutcome.Timeout:
                    return BpmCommandResult.Fail("Timeout aguardando resposta da BPM.", outcome);
                case SdGwLinkEngine.SendOutcome.Nacked:
                    return BpmCommandResult.Fail("A BPM rejeitou o ping solicitado.", outcome);
                case SdGwLinkEngine.SendOutcome.Busy:
                    return BpmCommandResult.Fail("O link da BPM estava temporariamente ocupado.", outcome);
                default:
                    return BpmCommandResult.Fail("Falha ao enviar o ping para a BPM.", outcome);
            }
        }
    }
}
