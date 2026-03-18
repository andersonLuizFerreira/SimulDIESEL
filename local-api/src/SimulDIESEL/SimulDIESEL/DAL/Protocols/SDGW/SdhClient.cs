using System;
using System.Threading.Tasks;
using SimulDIESEL.DTL.Protocols.SDGW;

namespace SimulDIESEL.DAL.Protocols.SDGW
{
    /// <summary>
    /// Camada semântica SDH sobre o protocolo SDGW.
    /// O link engine continua sendo infraestrutura técnica de frame/transporte;
    /// já o SDH traduz intenção funcional para comandos SDGW compactos.
    /// </summary>
    public sealed class SdhClient
    {
        private readonly SdgwSession _sdgw;
        private readonly SdhTextParser _parser;
        private readonly SdhTextSerializer _serializer;
        private readonly SdhValidator _validator;
        private readonly SdhToSdgwMapper _mapper;

        public SdhClient(SdgwSession sdgw)
            : this(sdgw, new SdhTextParser(), new SdhTextSerializer(), new SdhValidator(), new SdhToSdgwMapper())
        {
        }

        internal SdhClient(
            SdgwSession sdgw,
            SdhTextParser parser,
            SdhTextSerializer serializer,
            SdhValidator validator,
            SdhToSdgwMapper mapper)
        {
            _sdgw = sdgw ?? throw new ArgumentNullException(nameof(sdgw));
            _parser = parser ?? throw new ArgumentNullException(nameof(parser));
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _validator = validator ?? throw new ArgumentNullException(nameof(validator));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }

        public Task<SdGwLinkEngine.SendOutcome> SendAsync(SdhCommand command)
        {
            return SendAsync(command, SdGwTxPriority.Normal);
        }

        public Task<SdGwLinkEngine.SendOutcome> SendAsync(SdhCommand command, SdGwTxPriority priority, string origin = null)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            _validator.Validate(command);

            SdhToSdgwMapper.MappedSdgwCommand mapped = _mapper.Map(command);

            return _sdgw.SendAsync(
                mapped.Cmd,
                mapped.Payload,
                mapped.RequireAck,
                mapped.TimeoutMs,
                mapped.Retries,
                priority,
                origin ?? (command.Target + ":" + command.Op));
        }

        public Task<SdGwLinkEngine.SendOutcome> SendTextAsync(string text)
        {
            return SendAsync(_parser.Parse(text));
        }

        public string ToText(SdhCommand command)
        {
            if (command == null)
                throw new ArgumentNullException(nameof(command));

            return _serializer.Serialize(command);
        }
    }
}
