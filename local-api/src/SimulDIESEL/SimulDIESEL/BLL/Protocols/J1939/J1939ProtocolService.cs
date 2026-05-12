using System;
using System.Collections.Generic;
using SimulDIESEL.BLL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols.J1939
{
    public sealed class J1939ProtocolService
    {
        private readonly J1939DataLinkService _dataLinkService;

        public J1939ProtocolService()
            : this(new J1939DataLinkService())
        {
        }

        public J1939ProtocolService(J1939DataLinkService dataLinkService)
        {
            _dataLinkService = dataLinkService ?? throw new ArgumentNullException(nameof(dataLinkService));
        }

        public J1939DataLinkProcessingResultDto ProcessCanFrame(CanFrameDto frame)
        {
            return _dataLinkService.ProcessCanFrame(frame);
        }

        public J1939DataLinkProcessingResultDto ProcessCanRow(CanRowDto row)
        {
            return _dataLinkService.ProcessCanRow(row);
        }

        public IReadOnlyList<J1939DataLinkProcessingResultDto> ProcessSnapshot(IEnumerable<CanRowDto> rows)
        {
            return _dataLinkService.ProcessSnapshot(rows);
        }

        public IReadOnlyList<J1939TransportSessionDto> CheckTimeouts(DateTime now)
        {
            return _dataLinkService.CheckTimeouts(now);
        }
    }
}
