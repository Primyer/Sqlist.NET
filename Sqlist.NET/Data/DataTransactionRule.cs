namespace Sqlist.NET.Data
{
    public class DataTransactionRule
    {
        public string? ColumnName { get; set; }
        public string? Type { get; set; }
        public string? Cast { get; set; }
        public bool IsNew { get; set; }
    }
}
