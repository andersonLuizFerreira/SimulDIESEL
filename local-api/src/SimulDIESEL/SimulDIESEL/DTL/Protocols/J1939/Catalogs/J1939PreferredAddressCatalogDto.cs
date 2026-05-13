namespace SimulDIESEL.DTL.Protocols.J1939.Catalogs
{
    public sealed class J1939PreferredAddressCatalogDto
    {
        public string Id { get; set; }

        public int Address { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public int? FunctionCode { get; set; }

        public int? IndustryGroupCode { get; set; }

        public string Source { get; set; }

        public string Notes { get; set; }

        public bool IsKnown { get; set; }
    }
}
