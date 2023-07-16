namespace Sqlist.NET.Migration.Deserialization
{
    public class ColumnDefinition
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnDefinition"/> class.
        /// </summary>
        public ColumnDefinition()
        { }

        /// <summary>
        ///     Initializes a new instance of the <see cref="ColumnDefinition"/> class.
        /// </summary>
        public ColumnDefinition(string type) => Type = type;

        public bool IsSequence { get; set; }
        public string? SequenceName { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public bool IsEnum { get; set; }
    }
}
