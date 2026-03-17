using System.Collections.Generic;

namespace SimulDIESEL.DTL.Protocols.SDGW
{
    public sealed class SdhCommand
    {
        private Dictionary<string, string> _args;
        private Dictionary<string, string> _meta;

        public SdhCommand()
        {
            _args = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            _meta = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        }

        public string Version { get; set; } = "sdh/1";
        public string Target { get; set; }
        public string Op { get; set; }

        public Dictionary<string, string> Args
        {
            get { return _args; }
            set { _args = value ?? new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase); }
        }

        public Dictionary<string, string> Meta
        {
            get { return _meta; }
            set { _meta = value ?? new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase); }
        }
    }
}
