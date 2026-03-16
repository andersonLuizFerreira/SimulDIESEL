using System.Collections.Generic;

namespace SimulDIESEL.DTL
{
    public sealed class SdhResponse
    {
        private Dictionary<string, string> _data;
        private Dictionary<string, string> _meta;

        public SdhResponse()
        {
            _data = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
            _meta = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase);
        }

        public string Version { get; set; } = "sdh/1";
        public bool Ok { get; set; }
        public string Target { get; set; }
        public string Op { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }

        public Dictionary<string, string> Data
        {
            get { return _data; }
            set { _data = value ?? new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase); }
        }

        public Dictionary<string, string> Meta
        {
            get { return _meta; }
            set { _meta = value ?? new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase); }
        }
    }
}
