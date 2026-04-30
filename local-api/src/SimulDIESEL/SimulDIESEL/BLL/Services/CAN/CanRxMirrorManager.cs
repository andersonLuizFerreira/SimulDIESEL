using System.Collections.Generic;
using SimulDIESEL.DTL.Boards.UCE.Can;

namespace SimulDIESEL.BLL.Services.CAN
{
    public sealed class CanRxMirrorManager
    {
        private readonly Dictionary<int, CanRowDto> _rowsByIndex;

        public CanRxMirrorManager()
        {
            _rowsByIndex = new Dictionary<int, CanRowDto>();
        }

        public IReadOnlyCollection<CanRowDto> GetAll()
        {
            return _rowsByIndex.Values;
        }

        public CanRowDto GetById(int index)
        {
            CanRowDto row;
            return TryGetById(index, out row) ? row : null;
        }

        public bool TryGetById(int index, out CanRowDto row)
        {
            return _rowsByIndex.TryGetValue(index, out row);
        }

        public void ApplyCreate(CanCreateDto create)
        {
        }

        public void ApplyEdit(CanEditDto edit)
        {
        }

        public void ApplyDelete(CanDeleteDto delete)
        {
        }

        public void ReplaceAll(CanReadAllResponseDto response)
        {
        }
    }
}
