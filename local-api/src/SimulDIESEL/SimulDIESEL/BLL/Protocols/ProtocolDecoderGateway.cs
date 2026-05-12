using System;
using SimulDIESEL.BLL.Protocols.J1939;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;

namespace SimulDIESEL.BLL.Protocols
{
    public sealed class ProtocolDecoderGateway
    {
        private readonly J1939ProtocolService _j1939ProtocolService;

        public ProtocolDecoderGateway()
            : this(new J1939ProtocolService())
        {
        }

        public ProtocolDecoderGateway(J1939ProtocolService j1939ProtocolService)
        {
            _j1939ProtocolService = j1939ProtocolService ?? throw new ArgumentNullException(nameof(j1939ProtocolService));
        }

        public J1939DataLinkProcessingResultDto DecodeCanFrame(CanFrameDto frame)
        {
            if (frame == null)
                throw new ArgumentNullException(nameof(frame));

            return _j1939ProtocolService.ProcessCanFrame(frame);
        }

        public J1939DataLinkProcessingResultDto DecodeCanRow(CanRowDto row)
        {
            if (row == null)
                throw new ArgumentNullException(nameof(row));

            return _j1939ProtocolService.ProcessCanRow(row);
        }
    }
}
