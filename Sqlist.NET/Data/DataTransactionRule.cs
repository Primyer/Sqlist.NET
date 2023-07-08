namespace Sqlist.NET.Data
{
    public class DataTransactionRule
    {
        public string? ColumnName { get; set; }
        public string? CurrentType { get; set; }
        public string? Type { get; set; }
        public string? Value { get; set; }
        public bool IsNew { get; set; }
        public bool? IsEnum { get; set; }
    }
}
