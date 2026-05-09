using System;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.DTL.Boards.UCE;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    /// <summary>
    /// SDCTP event processor facade for CAN_CREATE, CAN_EDIT, CAN_DELETE, CAN_ROW,
    /// CAN_READ_ALL_DONE and CAN_TIC.
    /// </summary>
    public sealed class SdctpEventProcessor
    {
        private readonly CanEventProcessor _inner;

        public SdctpEventProcessor(SdctpRxMirrorManager mirrorManager)
            : this(new CanEventProcessor((mirrorManager ?? throw new ArgumentNullException(nameof(mirrorManager))).InnerCanRxMirrorManager))
        {
        }

        public SdctpEventProcessor(CanEventProcessor inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public bool ProcessEvent(byte type, byte[] payload)
        {
            return _inner.ProcessEvent(type, payload);
        }

        public void ProcessCanRxEvent(UceCanRxEvent canRxEvent)
        {
            _inner.ProcessCanRxEvent(canRxEvent);
        }
    }
}
