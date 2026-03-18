using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    public enum SdGwTxPriority
    {
        High = 0,
        Normal = 1,
        Low = 2
    }

    public sealed class SdGwTxScheduler : IDisposable
    {
        private sealed class QueueItem
        {
            public byte Cmd { get; set; }
            public byte[] Payload { get; set; }
            public SdGwLinkEngine.SendOptions Options { get; set; }
            public SdGwTxPriority Priority { get; set; }
            public string Origin { get; set; }
            public TaskCompletionSource<SdGwLinkEngine.SendOutcome> Completion { get; set; }
        }

        private readonly SdGwLinkEngine _engine;
        private readonly object _sync = new object();
        private readonly Queue<QueueItem> _high = new Queue<QueueItem>();
        private readonly Queue<QueueItem> _normal = new Queue<QueueItem>();
        private readonly Queue<QueueItem> _low = new Queue<QueueItem>();

        private bool _disposed;
        private bool _pumpRunning;
        private bool _transportAvailable;

        public SdGwTxScheduler(SdGwLinkEngine engine)
        {
            _engine = engine ?? throw new ArgumentNullException(nameof(engine));
        }

        public Task<SdGwLinkEngine.SendOutcome> EnqueueAsync(
            byte cmd,
            byte[] payload,
            SdGwLinkEngine.SendOptions options,
            SdGwTxPriority priority = SdGwTxPriority.Normal,
            string origin = null)
        {
            if (_disposed)
                return Task.FromResult(SdGwLinkEngine.SendOutcome.TransportDown);

            var item = new QueueItem
            {
                Cmd = cmd,
                Payload = payload ?? Array.Empty<byte>(),
                Options = CloneOptions(options),
                Priority = priority,
                Origin = origin ?? string.Empty,
                Completion = new TaskCompletionSource<SdGwLinkEngine.SendOutcome>(TaskCreationOptions.RunContinuationsAsynchronously)
            };

            bool shouldStartPump = false;

            lock (_sync)
            {
                if (_disposed || !_transportAvailable)
                    return Task.FromResult(SdGwLinkEngine.SendOutcome.TransportDown);

                Enqueue_NoLock(item);

                if (!_pumpRunning)
                {
                    _pumpRunning = true;
                    shouldStartPump = true;
                }
            }

            if (shouldStartPump)
                _ = Task.Run(ProcessQueueAsync);

            return item.Completion.Task;
        }

        public void SetTransportAvailable(bool available)
        {
            QueueItem[] pending = null;

            lock (_sync)
            {
                if (_disposed)
                    return;

                _transportAvailable = available;
                if (!available)
                    pending = DrainAll_NoLock();
            }

            CompletePending(pending, SdGwLinkEngine.SendOutcome.TransportDown);
        }

        private async Task ProcessQueueAsync()
        {
            while (true)
            {
                QueueItem item;

                lock (_sync)
                {
                    item = DequeueNext_NoLock();
                    if (item == null)
                    {
                        _pumpRunning = false;
                        return;
                    }
                }

                SdGwLinkEngine.SendOutcome outcome;

                try
                {
                    if (!_transportAvailable)
                    {
                        outcome = SdGwLinkEngine.SendOutcome.TransportDown;
                    }
                    else
                    {
                        outcome = await _engine.SendAsync(item.Cmd, item.Payload, item.Options).ConfigureAwait(false);
                    }
                }
                catch
                {
                    outcome = SdGwLinkEngine.SendOutcome.TransportDown;
                }

                item.Completion.TrySetResult(outcome);
            }
        }

        private void Enqueue_NoLock(QueueItem item)
        {
            switch (item.Priority)
            {
                case SdGwTxPriority.High:
                    _high.Enqueue(item);
                    break;
                case SdGwTxPriority.Low:
                    _low.Enqueue(item);
                    break;
                default:
                    _normal.Enqueue(item);
                    break;
            }
        }

        private QueueItem DequeueNext_NoLock()
        {
            if (_high.Count > 0)
                return _high.Dequeue();

            if (_normal.Count > 0)
                return _normal.Dequeue();

            if (_low.Count > 0)
                return _low.Dequeue();

            return null;
        }

        private QueueItem[] DrainAll_NoLock()
        {
            var pending = new List<QueueItem>(_high.Count + _normal.Count + _low.Count);

            DrainQueue_NoLock(_high, pending);
            DrainQueue_NoLock(_normal, pending);
            DrainQueue_NoLock(_low, pending);

            return pending.ToArray();
        }

        private static void DrainQueue_NoLock(Queue<QueueItem> source, List<QueueItem> destination)
        {
            while (source.Count > 0)
                destination.Add(source.Dequeue());
        }

        private static void CompletePending(IEnumerable<QueueItem> pending, SdGwLinkEngine.SendOutcome outcome)
        {
            if (pending == null)
                return;

            foreach (QueueItem item in pending)
                item.Completion.TrySetResult(outcome);
        }

        private static SdGwLinkEngine.SendOptions CloneOptions(SdGwLinkEngine.SendOptions options)
        {
            if (options == null)
                return new SdGwLinkEngine.SendOptions();

            return new SdGwLinkEngine.SendOptions
            {
                RequireAck = options.RequireAck,
                TimeoutMs = options.TimeoutMs,
                MaxRetries = options.MaxRetries,
                IsEvent = options.IsEvent,
                AdditionalFlags = options.AdditionalFlags
            };
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            QueueItem[] pending;

            lock (_sync)
            {
                if (_disposed)
                    return;

                _disposed = true;
                _transportAvailable = false;
                pending = DrainAll_NoLock();
            }

            CompletePending(pending, SdGwLinkEngine.SendOutcome.TransportDown);
        }
    }
}
