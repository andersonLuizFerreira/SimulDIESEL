using System;
using System.Threading.Tasks;
using SimulDIESEL.DTL;

namespace SimulDIESEL.BLL.SDH
{
    public sealed class SdhClient
    {
        private readonly SdGgwClient _sggw;
        private readonly SdhTextParser _parser;
        private readonly SdhTextSerializer _serializer;
        private readonly SdhValidator _validator;
        private readonly SdhToSggwMapper _mapper;

        public SdhClient(SdGgwClient sggw)
            : this(sggw, new SdhTextParser(), new SdhTextSerializer(), new SdhValidator(), new SdhToSggwMapper())
        {
        }

        internal SdhClient(
            SdGgwClient sggw,
            SdhTextParser parser,
            SdhTextSerializer serializer,
            SdhValidator validator,
            SdhToSggwMapper mapper)
        {
            _sggw = sggw ?? throw new ArgumentNullException(nameof(sggw));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public Task<SdGwLinkEngine.SendOutcome> SendAsync(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _validator.Validate(command);

            SdhToSggwMapper.MappedSggwCommand mapped = _mapper.Map(command);

            return _sggw.SendAsync(
                mapped.Cmd,
                mapped.Payload,
                mapped.RequireAck,
                mapped.TimeoutMs,
                mapped.Retries);
        }

        public Task<SdGwLinkEngine.SendOutcome> SendTextAsync(string text)
        {
            SdhCommand command = _parser.Parse(text);
            return SendAsync(command);
        }

        public string ToText(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return _serializer.Serialize(command);
        }
    }
}
