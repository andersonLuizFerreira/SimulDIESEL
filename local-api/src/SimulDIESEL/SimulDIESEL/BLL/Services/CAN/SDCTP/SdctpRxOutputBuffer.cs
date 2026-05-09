using System;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    /// <summary>
    /// SDCTP RX output buffer facade. This is the official consumer-facing RX queue.
    /// </summary>
    public sealed class SdctpRxOutputBuffer
    {
        private readonly CanRxOutputBuffer _inner;

        public SdctpRxOutputBuffer()
            : this(new CanRxOutputBuffer())
        {
        }

        public SdctpRxOutputBuffer(CanRxOutputBuffer inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public CanRxOutputBuffer InnerCanRxOutputBuffer { get { return _inner; } }
        public int Capacity { get { return _inner.Capacity; } }
        public int Count { get { return _inner.Count; } }
        public uint OverflowCount { get { return _inner.OverflowCount; } }

        public bool Enqueue(CanFrameDto frame)
        {
            return _inner.Enqueue(frame);
        }

        public bool TryReadRxFrame(out CanFrameDto frame)
        {
            return _inner.TryDequeue(out frame);
        }

        public void Clear()
        {
            _inner.Clear();
        }
    }
}
