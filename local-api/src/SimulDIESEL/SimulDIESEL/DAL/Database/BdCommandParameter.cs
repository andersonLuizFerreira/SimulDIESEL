namespace SimulDIESEL.DAL.Database
{
    public sealed class BdCommandParameter
    {
        public BdCommandParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; }

        public object Value { get; }
    }
}
