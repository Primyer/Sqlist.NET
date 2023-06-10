namespace Sqlist.NET.Sql
{
    public class NpgsqlEncloser : Encloser
    {
        public const char DI = '\"';

        public override string? Wrap(string? val)
        {
            return DI + val + DI;
        }

        public override string? Replace(string? val)
        {
            return val?.Replace('`', DI);
        }

        public override string? Reformat(string? val)
        {
            if (string.IsNullOrEmpty(val))
                return val;

            if (val.IndexOf("`") != -1)
                return Replace(val);

            if (val.IndexOf(' ') == -1)
                return Wrap(val);

            return val;
        }
    }
}
