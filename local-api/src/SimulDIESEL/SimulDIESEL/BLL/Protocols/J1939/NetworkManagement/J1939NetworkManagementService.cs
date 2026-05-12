using System;
using SimulDIESEL.DTL.Protocols.J1939.DataLink;
using SimulDIESEL.DTL.Protocols.J1939.NetworkManagement;

namespace SimulDIESEL.BLL.Protocols.J1939.NetworkManagement
{
    public sealed class J1939NetworkManagementService
    {
        private readonly J1939AddressClaimDecoder _addressClaimDecoder;
        private readonly J1939AddressRegistry _addressRegistry;
        private readonly J1939WorkingSetService _workingSetService;

        public J1939NetworkManagementService()
            : this(new J1939AddressClaimDecoder(), new J1939AddressRegistry(), new J1939WorkingSetService())
        {
        }

        public J1939NetworkManagementService(
            J1939AddressClaimDecoder addressClaimDecoder,
            J1939AddressRegistry addressRegistry,
            J1939WorkingSetService workingSetService)
        {
            _addressClaimDecoder = addressClaimDecoder ?? throw new ArgumentNullException(nameof(addressClaimDecoder));
            _addressRegistry = addressRegistry ?? throw new ArgumentNullException(nameof(addressRegistry));
            _workingSetService = workingSetService ?? throw new ArgumentNullException(nameof(workingSetService));
        }

        public J1939AddressRegistry AddressRegistry
        {
            get { return _addressRegistry; }
        }

        public bool TryProcess(J1939DataLinkProcessingResultDto result, out J1939NetworkEventDto networkEvent)
        {
            networkEvent = null;
            if (result == null || result.SingleFrameMessage == null)
                return false;

            return TryProcess(result.SingleFrameMessage, out networkEvent);
        }

        public bool TryProcess(J1939DataLinkMessageDto message, out J1939NetworkEventDto networkEvent)
        {
            networkEvent = null;
            if (message == null)
                return false;

            if (_addressClaimDecoder.CanDecode(message))
            {
                J1939AddressClaimDto claim = _addressClaimDecoder.Decode(message);
                networkEvent = _addressRegistry.Apply(claim);
                return true;
            }

            J1939WorkingSetDto workingSet;
            if (_workingSetService.TryDecodeMaster(message, out workingSet))
            {
                networkEvent = new J1939NetworkEventDto
                {
                    EventType = "WorkingSetMaster",
                    SourceAddress = workingSet.SourceAddress,
                    WorkingSet = workingSet,
                    Status = workingSet.Status,
                    Timestamp = message.Timestamp == default(DateTime) ? DateTime.Now : message.Timestamp
                };
                return true;
            }

            J1939WorkingSetMemberDto member;
            if (_workingSetService.TryDecodeMember(message, out member))
            {
                networkEvent = new J1939NetworkEventDto
                {
                    EventType = "WorkingSetMember",
                    SourceAddress = member.SourceAddress,
                    WorkingSetMember = member,
                    Status = member.Status,
                    Timestamp = message.Timestamp == default(DateTime) ? DateTime.Now : message.Timestamp
                };
                return true;
            }

            return false;
        }

        public bool TryProcess(J1939ReassembledMessageDto message, out J1939NetworkEventDto networkEvent)
        {
            networkEvent = null;
            return false;
        }
    }
}
