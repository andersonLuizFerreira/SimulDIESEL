using System;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

namespace SimulDIESEL.BLL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939WorkingSetService
    {
        public const uint WorkingSetMasterPgn = 65037;
        public const uint WorkingSetMemberPgn = 65036;

        private readonly J1939NameParser _nameParser;

        public J1939WorkingSetService()
            : this(new J1939NameParser())
        {
        }

        public J1939WorkingSetService(J1939NameParser nameParser)
        {
            _nameParser = nameParser ?? throw new ArgumentNullException(nameof(nameParser));
        }

        public bool TryDecodeMaster(J1939DataLinkMessageDto message, out J1939WorkingSetDto workingSet)
        {
            workingSet = null;
            if (message == null || message.Pgn != WorkingSetMasterPgn || message.Data == null || message.Dlc < 1)
                return false;

            workingSet = new J1939WorkingSetDto
            {
                SourceAddress = message.IdFields != null ? message.IdFields.SourceAddress : (byte)0,
                MemberCount = message.Data[0],
                Status = "WorkingSetMasterDecoded"
            };
            return true;
        }

        public bool TryDecodeMember(J1939DataLinkMessageDto message, out J1939WorkingSetMemberDto member)
        {
            member = null;
            if (message == null || message.Pgn != WorkingSetMemberPgn || message.Data == null || message.Dlc < 8)
                return false;

            member = new J1939WorkingSetMemberDto
            {
                SourceAddress = message.IdFields != null ? message.IdFields.SourceAddress : (byte)0,
                Name = _nameParser.Parse(message.Data),
                Status = "WorkingSetMemberDecoded"
            };
            return true;
        }
    }
}
