using System;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Protocols.SDCTP;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    /// <summary>
    /// SDCTP event processor facade for CAN_CREATE, CAN_EDIT, CAN_DELETE, CAN_ROW,
    /// legacy CAN_READ_ALL_DONE and CAN_TIC.
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

        public bool ProcessEvent(SdctpRawEventDto rawEvent)
        {
            if (rawEvent == null || rawEvent.Type == GwProtocol.UceCanRxEventType)
                return false;

            return _inner.ProcessEvent(rawEvent.Type, rawEvent.Payload);
        }

        public bool TryReadCanRxEvent(SdctpRawEventDto rawEvent, out UceCanRxEvent canRxEvent, out string error)
        {
            return SdctpEventParser.TryReadCanRxEvent(rawEvent, out canRxEvent, out error);
        }

        public void ProcessCanRxEvent(UceCanRxEvent canRxEvent)
        {
            _inner.ProcessCanRxEvent(canRxEvent);
        }
    }
}
