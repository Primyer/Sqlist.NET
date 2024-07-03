namespace Sqlist.NET.Sql;
public sealed class DummyEncloser : Encloser
{
    public override string? Wrap(string? val) => val;
    public override string? Replace(string? val)
    {
        return val?.Replace("`", "");
    }
}