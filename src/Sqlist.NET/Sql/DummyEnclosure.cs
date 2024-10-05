namespace Sqlist.NET.Sql;
public sealed class DummyEnclosure : Enclosure
{
    public override string? Wrap(string? val) => val;
    public override string? Replace(string? val)
    {
        return val?.Replace("`", "");
    }
}