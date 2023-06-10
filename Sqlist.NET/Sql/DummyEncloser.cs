namespace Sqlist.NET.Sql
{
    public sealed class DummyEncloser : Encloser
    {
        public override string? Reformat(string? val) => val;
        public override string? Replace(string? val) => val;
        public override string? Wrap(string? val) => val;
    }
}
