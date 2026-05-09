using System;
using System.Collections.Generic;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN
{
    /// <summary>
    /// Official validated SDCTP API RX output queue.
    /// </summary>
    public sealed class CanRxOutputBuffer
    {
        public const int DefaultCapacity = 256;

        private readonly object _sync = new object();
        private readonly Queue<CanFrameDto> _frames;
        private readonly int _capacity;
        private uint _overflowCount;

        public CanRxOutputBuffer()
            : this(DefaultCapacity)
        {
        }

        public CanRxOutputBuffer(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException(nameof(capacity));

            _capacity = capacity;
            _frames = new Queue<CanFrameDto>(capacity);
        }

        public int Capacity
        {
            get { return _capacity; }
        }

        public int Count
        {
            get
            {
                lock (_sync)
                {
                    return _frames.Count;
                }
            }
        }

        public uint OverflowCount
        {
            get
            {
                lock (_sync)
                {
                    return _overflowCount;
                }
            }
        }

        public bool Enqueue(CanFrameDto frame)
        {
            if (frame == null)
                return false;

            lock (_sync)
            {
                if (_frames.Count >= _capacity)
                {
                    ++_overflowCount;
                    return false;
                }

                _frames.Enqueue(frame.Clone());
                return true;
            }
        }

        public bool TryDequeue(out CanFrameDto frame)
        {
            lock (_sync)
            {
                if (_frames.Count == 0)
                {
                    frame = null;
                    return false;
                }

                frame = _frames.Dequeue().Clone();
                return true;
            }
        }

        public void Clear()
        {
            lock (_sync)
            {
                _frames.Clear();
            }
        }
    }
}
