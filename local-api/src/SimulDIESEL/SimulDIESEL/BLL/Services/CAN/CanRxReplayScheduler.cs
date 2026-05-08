using System;
using System.Collections.Generic;
using System.Threading;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN
{
    [Obsolete("Legado: o fluxo normal de RX agora e alimentado por CAN_CREATE/CAN_EDIT/CAN_TIC/CAN_RX_EVENT, sem replay local por cycleTime.")]
    public sealed class CanRxReplayScheduler : IDisposable
    {
        private const int TickPeriodMs = 50;

        private readonly CanRxMirrorManager _mirrorManager;
        private readonly CanRxOutputBuffer _outputBuffer;
        private readonly Func<bool> _isPaused;
        private readonly object _sync = new object();
        private readonly Dictionary<int, DateTime> _nextDueByIndex = new Dictionary<int, DateTime>();
        private Timer _timer;
        private bool _disposed;

        public CanRxReplayScheduler(
            CanRxMirrorManager mirrorManager,
            CanRxOutputBuffer outputBuffer,
            Func<bool> isPaused)
        {
            _mirrorManager = mirrorManager ?? throw new ArgumentNullException(nameof(mirrorManager));
            _outputBuffer = outputBuffer ?? throw new ArgumentNullException(nameof(outputBuffer));
            _isPaused = isPaused ?? (() => false);
        }

        public event Action<CanFrameDto> FrameReplayed;

        public void Start()
        {
            lock (_sync)
            {
                if (_disposed || _timer != null)
                    return;

                _timer = new Timer(OnTick, null, TickPeriodMs, TickPeriodMs);
            }
        }

        public void Stop()
        {
            lock (_sync)
            {
                if (_timer == null)
                    return;

                _timer.Dispose();
                _timer = null;
            }
        }

        public void ClearSchedule()
        {
            lock (_sync)
            {
                _nextDueByIndex.Clear();
            }
        }

        public void RemoveSchedule(int index)
        {
            lock (_sync)
            {
                _nextDueByIndex.Remove(index);
            }
        }

        private void OnTick(object state)
        {
            if (_disposed || _isPaused())
                return;

            DateTime now = DateTime.Now;
            IReadOnlyList<CanRowDto> snapshot = _mirrorManager.GetSnapshot();
            for (int index = 0; index < snapshot.Count; ++index)
            {
                CanRowDto row = snapshot[index];
                if (row == null || !row.Valid || row.CycleTime == 0)
                {
                    RemoveSchedule(index);
                    continue;
                }

                DateTime nextDue;
                bool shouldReplay = false;
                lock (_sync)
                {
                    if (!_nextDueByIndex.TryGetValue(index, out nextDue))
                    {
                        _nextDueByIndex[index] = now.AddMilliseconds(row.CycleTime);
                        continue;
                    }

                    if (now >= nextDue)
                    {
                        shouldReplay = true;
                        _nextDueByIndex[index] = now.AddMilliseconds(row.CycleTime);
                    }
                }

                if (shouldReplay)
                    Replay(row);
            }
        }

        private void Replay(CanRowDto row)
        {
            CanFrameDto frame = ToFrame(row, CanFrameSource.Replay);
            if (_outputBuffer.Enqueue(frame))
                FrameReplayed?.Invoke(frame);
        }

        private static CanFrameDto ToFrame(CanRowDto row, CanFrameSource source)
        {
            return new CanFrameDto
            {
                CanId = row.CanId,
                IsExtended = row.IsExtended,
                IsRemoteRequest = row.IsRemoteRequest,
                Dlc = row.Dlc,
                Data = row.Data != null ? (byte[])row.Data.Clone() : new byte[8],
                Timestamp = DateTime.Now,
                Source = source
            };
        }

        public void Dispose()
        {
            _disposed = true;
            Stop();
        }
    }
}
