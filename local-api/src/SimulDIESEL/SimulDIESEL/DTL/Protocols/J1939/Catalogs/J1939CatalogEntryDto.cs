namespace SimulDIESEL.DTL.Protocols.J1939.Catalogs
{
    public class J1939CatalogEntryDto
    {
        public string Id { get; set; }

        public int Code { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Source { get; set; }

        public string Notes { get; set; }

        public bool IsKnown { get; set; }
    }
}
