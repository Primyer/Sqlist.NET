namespace Sqlist.NET.Data;
public class TransactionRuleDictionary : Dictionary<string, DataTransactionRule>
{
    public string? Condition { get; set; }
}