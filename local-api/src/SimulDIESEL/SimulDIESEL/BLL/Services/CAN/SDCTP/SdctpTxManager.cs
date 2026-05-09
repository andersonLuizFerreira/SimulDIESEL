using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    /// <summary>
    /// SDCTP TX facade. DIRECT sends immediately; TABLE commands create/edit/delete
    /// cyclic rows executed locally by the UCE.
    /// </summary>
    public sealed class SdctpTxManager
    {
        private readonly CanTxManager _inner;

        public SdctpTxManager(IUceDispatcher uceDispatcher)
            : this(new CanTxManager(uceDispatcher))
        {
        }

        public SdctpTxManager(CanTxManager inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendFrameAsync(string controller, CanFrameDto frame)
        {
            return _inner.SendFrameAsync(controller, frame);
        }

        public Task<UceOperationResult<UceCanTxResponse>> CreateTxRowAsync(string controller, int index, CanFrameDto frame, ushort periodMs, bool enabled)
        {
            return _inner.CreateTxRowAsync(controller, index, frame, periodMs, enabled);
        }

        public Task<UceOperationResult<UceCanTxResponse>> EditTxRowAsync(string controller, int index, CanFrameDto frame, ushort? periodMs, bool? enabled)
        {
            return _inner.EditTxRowAsync(controller, index, frame, periodMs, enabled);
        }

        public Task<UceOperationResult<UceCanTxResponse>> DeleteTxRowAsync(string controller, int index, byte reason)
        {
            return _inner.DeleteTxRowAsync(controller, index, reason);
        }

        public IReadOnlyList<CanTxRowDto> GetTxSnapshot()
        {
            return _inner.GetTxSnapshot();
        }
    }
}
