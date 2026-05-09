using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SimulDIESEL.BLL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Services.CAN
{
    /// <summary>
    /// Validated SDCTP API TX manager for DIRECT and TABLE commands.
    /// </summary>
    public sealed class CanTxManager
    {
        private const int Capacity = 100;
        private const byte DefaultCyclicIndex = 0;

        private readonly object _sync = new object();
        private readonly IUceDispatcher _uceDispatcher;
        private readonly CanTxRowDto[] _rows = new CanTxRowDto[Capacity];

        public CanTxManager(IUceDispatcher uceDispatcher)
        {
            _uceDispatcher = uceDispatcher;
        }

        public Task<UceOperationResult<UceCanTxResponse>> SendFrameAsync(string controller, CanFrameDto frame)
        {
            byte[] data = NormalizeData(frame);
            return _uceDispatcher.SendCanDirectAsync(
                controller,
                frame != null && frame.IsExtended,
                frame != null && frame.IsRemoteRequest,
                frame == null ? 0U : frame.CanId,
                frame == null ? (byte)0 : frame.Dlc,
                data);
        }

        public Task<UceOperationResult<UceCanTxResponse>> StartCyclicTxAsync(string controller, CanFrameDto frame, ushort periodMs)
        {
            return CreateTxRowAsync(controller, DefaultCyclicIndex, frame, periodMs, true);
        }

        public async Task<UceOperationResult<UceCanTxStopResponse>> StopTxAsync(string controller)
        {
            UceOperationResult<UceCanTxResponse> result = await DeleteTxRowAsync(
                controller,
                DefaultCyclicIndex,
                GwProtocol.UceCanTxDeleteReasonUserDelete).ConfigureAwait(false);

            if (!result.Success || result.Response == null)
                return UceOperationResult<UceCanTxStopResponse>.Fail(result.Message, result.SendOutcome);

            return UceOperationResult<UceCanTxStopResponse>.Succeeded(
                new UceCanTxStopResponse
                {
                    Controller = result.Response.Controller,
                    TxStatus = result.Response.TxStatus
                },
                result.SendOutcome ?? SimulDIESEL.DAL.Protocols.SDGW.SdGwLinkEngine.SendOutcome.Acked,
                result.Message);
        }

        public void CreateMessage(CanCreateDto create)
        {
        }

        public void EditMessage(CanEditDto edit)
        {
        }

        public void RemoveMessage(CanDeleteDto delete)
        {
        }

        public async Task<UceOperationResult<UceCanTxResponse>> CreateTxRowAsync(string controller, int index, CanFrameDto frame, ushort periodMs, bool enabled)
        {
            ValidateIndex(index);
            byte[] data = NormalizeData(frame);
            UceOperationResult<UceCanTxResponse> result = await _uceDispatcher.CreateCanTxRowAsync(
                controller,
                checked((byte)index),
                frame != null && frame.IsExtended,
                frame != null && frame.IsRemoteRequest,
                frame == null ? 0U : frame.CanId,
                frame == null ? (byte)0 : frame.Dlc,
                data,
                periodMs,
                enabled).ConfigureAwait(false);

            if (result.Success)
                StoreRow(index, frame, periodMs, enabled);

            return result;
        }

        public async Task<UceOperationResult<UceCanTxResponse>> EditTxRowAsync(string controller, int index, CanFrameDto frame, ushort? periodMs, bool? enabled)
        {
            ValidateIndex(index);

            CanTxRowDto current;
            lock (_sync)
            {
                current = _rows[index] != null ? _rows[index].Clone() : null;
            }

            if (current == null || !current.Valid)
                return UceOperationResult<UceCanTxResponse>.Fail("Linha CAN_TX inexistente no espelho local da API.");

            byte mask = 0;
            byte flags = EncodeFlags(current.IsExtended, current.IsRemoteRequest);
            uint id = current.CanId;
            byte dlc = current.Dlc;
            byte[] data = current.Data != null ? (byte[])current.Data.Clone() : new byte[8];
            ushort effectivePeriodMs = current.PeriodMs;
            bool effectiveEnabled = current.Enabled;
            byte dataMask = 0;

            if (frame != null)
            {
                byte newFlags = EncodeFlags(frame.IsExtended, frame.IsRemoteRequest);
                if (newFlags != flags)
                {
                    flags = newFlags;
                    mask |= GwProtocol.UceCanTxEditMaskFlags;
                }

                if (frame.CanId != id)
                {
                    id = frame.CanId;
                    mask |= GwProtocol.UceCanTxEditMaskCanId;
                }

                if (frame.Dlc != dlc)
                {
                    dlc = frame.Dlc;
                    mask |= GwProtocol.UceCanTxEditMaskDlc;
                }

                byte[] newData = NormalizeData(frame);
                for (int dataIndex = 0; dataIndex < 8; ++dataIndex)
                {
                    if (newData[dataIndex] == data[dataIndex])
                        continue;

                    data[dataIndex] = newData[dataIndex];
                    dataMask |= (byte)(1 << dataIndex);
                }

                if (dataMask != 0)
                    mask |= GwProtocol.UceCanTxEditMaskData;
            }

            if (periodMs.HasValue && periodMs.Value != effectivePeriodMs)
            {
                effectivePeriodMs = periodMs.Value;
                mask |= GwProtocol.UceCanTxEditMaskPeriodMs;
                if (effectivePeriodMs == 0)
                    effectiveEnabled = false;
            }

            if (enabled.HasValue && enabled.Value != effectiveEnabled)
            {
                effectiveEnabled = enabled.Value;
                mask |= GwProtocol.UceCanTxEditMaskEnabled;
            }

            if (mask == 0)
                return UceOperationResult<UceCanTxResponse>.Succeeded(new UceCanTxResponse(), SimulDIESEL.DAL.Protocols.SDGW.SdGwLinkEngine.SendOutcome.Acked, "Nenhuma alteração CAN_TX necessária.");

            UceOperationResult<UceCanTxResponse> result = await _uceDispatcher.EditCanTxRowAsync(
                controller,
                checked((byte)index),
                mask,
                flags,
                id,
                dlc,
                dataMask,
                data,
                effectivePeriodMs,
                effectiveEnabled).ConfigureAwait(false);

            if (result.Success)
                StoreRow(index, flags, id, dlc, data, effectivePeriodMs, effectiveEnabled);

            return result;
        }

        public async Task<UceOperationResult<UceCanTxResponse>> DeleteTxRowAsync(string controller, int index, byte reason)
        {
            ValidateIndex(index);
            UceOperationResult<UceCanTxResponse> result = await _uceDispatcher.DeleteCanTxRowAsync(
                controller,
                checked((byte)index),
                reason).ConfigureAwait(false);

            if (result.Success)
            {
                lock (_sync)
                    _rows[index] = null;
            }

            return result;
        }

        public Task<UceOperationResult<UceCanTxResponse>> EnableTxRowAsync(string controller, int index, bool enabled)
        {
            return EditTxRowAsync(controller, index, null, null, enabled);
        }

        public IReadOnlyList<CanTxRowDto> GetTxSnapshot()
        {
            var snapshot = new List<CanTxRowDto>();
            lock (_sync)
            {
                for (int index = 0; index < _rows.Length; ++index)
                {
                    if (_rows[index] != null && _rows[index].Valid)
                        snapshot.Add(_rows[index].Clone());
                }
            }

            return snapshot;
        }

        private void StoreRow(int index, CanFrameDto frame, ushort periodMs, bool enabled)
        {
            StoreRow(
                index,
                EncodeFlags(frame != null && frame.IsExtended, frame != null && frame.IsRemoteRequest),
                frame == null ? 0U : frame.CanId,
                frame == null ? (byte)0 : frame.Dlc,
                NormalizeData(frame),
                periodMs,
                enabled);
        }

        private void StoreRow(int index, byte flags, uint id, byte dlc, byte[] data, ushort periodMs, bool enabled)
        {
            lock (_sync)
            {
                _rows[index] = new CanTxRowDto
                {
                    Index = index,
                    Valid = true,
                    Enabled = enabled && periodMs > 0,
                    CanId = id,
                    IsExtended = (flags & 0x01) != 0,
                    IsRemoteRequest = (flags & 0x02) != 0,
                    Dlc = dlc,
                    Data = data != null ? (byte[])data.Clone() : new byte[8],
                    PeriodMs = periodMs
                };
            }
        }

        private static byte[] NormalizeData(CanFrameDto frame)
        {
            byte[] data = new byte[8];
            if (frame != null && frame.Data != null)
                Array.Copy(frame.Data, data, Math.Min(8, frame.Data.Length));

            return data;
        }

        private static byte EncodeFlags(bool extended, bool rtr)
        {
            return (byte)((extended ? 0x01 : 0x00) | (rtr ? 0x02 : 0x00));
        }

        private static void ValidateIndex(int index)
        {
            if (index < 0 || index >= Capacity)
                throw new ArgumentOutOfRangeException(nameof(index), "Index CAN_TX deve estar entre 0 e 99.");
        }
    }
}
