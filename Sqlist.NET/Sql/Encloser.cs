namespace Sqlist.NET.Sql
{
    public abstract class Encloser
    {
        public static Encloser? Default { get; internal set; }

        public abstract string? Wrap(string? val);
        public abstract string? Replace(string? val);

        public virtual string? Reformat(string? val)
        {
            if (string.IsNullOrEmpty(val))
                return val;

            if (val.Contains('`'))
                return Replace(val);

            if (val.IndexOfAny(new[] { ' ', '@' }) == -1)
                return Wrap(val);

            return val;
        }

        public virtual string? Join(string delimiter, params string[] vals)
        {
            var result = string.Empty;
            for (int i = 0; i < vals.Length; i++)
            {
                result += Wrap(vals[i]);

                if (i != vals.Length - 1)
                    result += delimiter;
            }
            return result;
        }
    }
}
