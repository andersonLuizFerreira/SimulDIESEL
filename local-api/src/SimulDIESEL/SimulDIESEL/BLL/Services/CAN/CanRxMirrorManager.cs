using System;
using System.Collections.Generic;
using System.Diagnostics;
using SimulDIESEL.DTL.Boards.UCE.Can;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.BLL.Services.CAN
{
    public sealed class CanRxMirrorManager
    {
        private readonly CanRowDto[] _rows;
        private bool _isSyncingReadAll;

        public CanRxMirrorManager()
        {
            _rows = new CanRowDto[GwProtocol.UceCanRxMirrorCapacity];
            for (int index = 0; index < _rows.Length; ++index)
            {
                _rows[index] = new CanRowDto
                {
                    Index = index,
                    Valid = false
                };
            }
        }

        public IReadOnlyList<CanRowDto> GetAll()
        {
            var snapshot = new CanRowDto[_rows.Length];
            for (int index = 0; index < _rows.Length; ++index)
                snapshot[index] = _rows[index].Clone();

            return snapshot;
        }

        public bool IsSyncingReadAll
        {
            get { return _isSyncingReadAll; }
        }

        public CanRowDto GetById(int index)
        {
            CanRowDto row;
            return TryGetById(index, out row) ? row : null;
        }

        public bool TryGetById(int index, out CanRowDto row)
        {
            row = null;
            if (!IsValidIndex(index))
                return false;

            row = _rows[index].Clone();
            return true;
        }

        public bool ApplyCreate(CanCreateDto create)
        {
            if (_isSyncingReadAll)
                return false;

            if (create == null || !IsValidIndex(create.Index))
            {
                Debug.WriteLine("CanRxMirrorManager.ApplyCreate ignorado: payload nulo ou index fora da faixa.");
                return false;
            }

            CanRowDto row = _rows[create.Index];
            row.Valid = true;
            row.Flags = create.Flags;
            row.CanId = create.CanId;
            row.Dlc = create.Dlc;
            row.CycleTime = create.CycleTime;
            row.MessageOrder = create.MessageOrder;
            EnsureDataArray(row);
            CopyEightBytes(create.Data, row.Data);
            return true;
        }

        public bool ApplyEdit(CanEditDto edit)
        {
            if (_isSyncingReadAll)
                return false;

            if (edit == null || !IsValidIndex(edit.Index))
            {
                Debug.WriteLine("CanRxMirrorManager.ApplyEdit ignorado: payload nulo ou index fora da faixa.");
                return false;
            }

            CanRowDto row = _rows[edit.Index];
            if (!row.Valid)
            {
                Debug.WriteLine("CanRxMirrorManager.ApplyEdit ignorado: linha ainda não criada para index " + edit.Index + ".");
                return false;
            }

            if ((edit.Mask & GwProtocol.UceCanCrudEditMaskFlags) != 0)
                row.Flags = edit.Flags;

            if ((edit.Mask & GwProtocol.UceCanCrudEditMaskCanId) != 0)
                row.CanId = edit.CanId;

            if ((edit.Mask & GwProtocol.UceCanCrudEditMaskDlc) != 0)
                row.Dlc = edit.Dlc;

            if ((edit.Mask & GwProtocol.UceCanCrudEditMaskData) != 0)
            {
                EnsureDataArray(row);
                CopyEightBytes(edit.Data, row.Data);
            }

            if ((edit.Mask & GwProtocol.UceCanCrudEditMaskCycleTime) != 0)
                row.CycleTime = edit.CycleTime;

            row.MessageOrder = edit.MessageOrder;
            return true;
        }

        public void StartReadAll()
        {
            ClearRows();
            _isSyncingReadAll = true;
            Debug.WriteLine("CanRxMirrorManager.StartReadAll: tabela espelho limpa e sincronização iniciada.");
        }

        public void CancelReadAll()
        {
            _isSyncingReadAll = false;
            Debug.WriteLine("CanRxMirrorManager.CancelReadAll: sincronização READ_ALL cancelada.");
        }

        public bool ApplyRow(CanRowDto dto)
        {
            if (dto == null || !IsValidIndex(dto.Index))
            {
                Debug.WriteLine("CanRxMirrorManager.ApplyRow ignorado: payload nulo ou index fora da faixa.");
                return false;
            }

            CanRowDto row = _rows[dto.Index];
            row.Valid = true;
            row.Flags = dto.Flags;
            row.CanId = dto.CanId;
            row.Dlc = dto.Dlc;
            row.CycleTime = dto.CycleTime;
            row.MessageOrder = dto.MessageOrder;
            EnsureDataArray(row);
            CopyEightBytes(dto.Data, row.Data);
            Debug.WriteLine(
                "CanRxMirrorManager.ApplyRow index=" +
                dto.Index.ToString() +
                " messageOrder=" +
                dto.MessageOrder.ToString() +
                ".");
            return true;
        }

        public bool ApplyReadAllDone(int count, uint messageOrder)
        {
            _isSyncingReadAll = false;
            Debug.WriteLine(
                "CanRxMirrorManager.ApplyReadAllDone count=" +
                count.ToString() +
                " messageOrder=" +
                messageOrder.ToString() +
                ".");
            return true;
        }

        public bool ApplyDelete(CanDeleteDto delete)
        {
            Debug.WriteLine("CanRxMirrorManager.ApplyDelete não implementado nesta etapa.");
            return false;
        }

        public bool ReplaceAll(CanReadAllResponseDto response)
        {
            if (response == null)
                return false;

            ClearRows();

            foreach (CanRowDto row in response.Rows)
            {
                if (row == null || !IsValidIndex(row.Index))
                    continue;

                _rows[row.Index] = row.Clone();
            }

            return true;
        }

        private void ClearRows()
        {
            for (int index = 0; index < _rows.Length; ++index)
            {
                _rows[index].Valid = false;
                _rows[index].Flags = 0;
                _rows[index].CanId = 0;
                _rows[index].Dlc = 0;
                _rows[index].CycleTime = 0;
                _rows[index].MessageOrder = 0;
                EnsureDataArray(_rows[index]);
                Array.Clear(_rows[index].Data, 0, _rows[index].Data.Length);
            }
        }

        private static bool IsValidIndex(int index)
        {
            return index >= 0 && index < GwProtocol.UceCanRxMirrorCapacity;
        }

        private static void EnsureDataArray(CanRowDto row)
        {
            if (row.Data == null || row.Data.Length != 8)
                row.Data = new byte[8];
        }

        private static void CopyEightBytes(byte[] source, byte[] destination)
        {
            Array.Clear(destination, 0, destination.Length);
            if (source == null)
                return;

            Buffer.BlockCopy(source, 0, destination, 0, Math.Min(8, source.Length));
        }
    }
}
