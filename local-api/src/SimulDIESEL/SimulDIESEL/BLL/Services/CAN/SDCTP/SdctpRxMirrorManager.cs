using System;
using System.Collections.Generic;
using SimulDIESEL.BLL.Services.CAN;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN.SDCTP
{
    /// <summary>
    /// SDCTP RX mirror facade over the validated CAN mirror manager.
    /// The mirror is internal protocol state; consumers should read frames via TryReadRxFrame.
    /// </summary>
    public sealed class SdctpRxMirrorManager
    {
        private readonly CanRxMirrorManager _inner;

        public SdctpRxMirrorManager()
            : this(new CanRxMirrorManager())
        {
        }

        public SdctpRxMirrorManager(CanRxMirrorManager inner)
        {
            _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        }

        public CanRxMirrorManager InnerCanRxMirrorManager { get { return _inner; } }
        public bool IsSyncingReadAll { get { return _inner.IsSyncingReadAll; } }

        public event Action<int, string> MirrorOutOfSyncDetected
        {
            add { _inner.MirrorOutOfSyncDetected += value; }
            remove { _inner.MirrorOutOfSyncDetected -= value; }
        }

        public IReadOnlyList<CanRowDto> GetSnapshot()
        {
            return _inner.GetSnapshot();
        }

        public bool TryGetById(int index, out CanRowDto row)
        {
            return _inner.TryGetById(index, out row);
        }

        public void StartReadAll()
        {
            _inner.StartReadAll();
        }

        public void StartReadAll(bool clearRows)
        {
            _inner.StartReadAll(clearRows);
        }

        public void CancelReadAll()
        {
            _inner.CancelReadAll();
        }

        public void ClearAll()
        {
            _inner.ClearAll();
        }
    }
}
