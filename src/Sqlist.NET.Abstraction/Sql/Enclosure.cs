namespace Sqlist.NET.Sql
{
    public abstract class Enclosure
    {
        public static Enclosure? Default { get; internal set; }

        public abstract string? Wrap(string? val);
        public abstract string? Replace(string? val);

        public virtual string? Reformat(string? val)
        {
            if (string.IsNullOrEmpty(val))
                return val;

            if (val.Contains('`'))
                return Replace(val);

            return val.IndexOfAny([' ', '@']) == -1 ? Wrap(val) : val;
        }

        public string Join(string delimiter, params string[] vals)
        {
            var result = string.Empty;
            for (var i = 0; i < vals.Length; i++)
            {
                result += Wrap(vals[i]);

                if (i != vals.Length - 1)
                    result += delimiter;
            }
            return result;
        }
    }
}
