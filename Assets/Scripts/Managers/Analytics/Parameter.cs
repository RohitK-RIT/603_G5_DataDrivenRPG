namespace Core.Managers.Analytics
{
    public struct Parameter
    {
        public string Name { get; private set; }
        public string Value { get; private set; }

        public Parameter(string name, string value)
        {
            Name = name;
            Value = value;
        }
    }
}