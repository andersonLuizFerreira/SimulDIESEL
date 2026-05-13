namespace SimulDIESEL.DTL.Protocols.J1939.Catalogs
{
    public sealed class J1939NameFieldDefinitionCatalogDto
    {
        public string Id { get; set; }

        public string FieldName { get; set; }

        public int? BitStart { get; set; }

        public int? BitLength { get; set; }

        public string Description { get; set; }

        public string Source { get; set; }

        public string Notes { get; set; }

        public bool IsKnown { get; set; }
    }
}
