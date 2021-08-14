namespace Sqlist.NET.Utilities
{
    public class Encloser
    {
        private SqlStyle _style;

        private char _openingDI; // Opening delimited identifier.
        private char _closingDI; // Closing delimited identifier.

        public Encloser(SqlStyle style)
        {
            Style = style;
        }

        public SqlStyle Style
        {
            get => _style;
            set
            {
                SetDIs(value);
                _style = value;
            }
        }

        public string Wrap(string val)
        {
            return _openingDI + val + _closingDI;
        }

        public void Wrap(ref string val)
        {
            val = _openingDI + val + _closingDI;
        }

        public void Reformat(ref string val)
        {
            if (val.IndexOf("[") != -1)
                val = val.Replace('[', _openingDI).Replace(']', _closingDI);
            else
                Wrap(ref val);
        }

        public string Reformat(string val)
        {
            Reformat(ref val);
            return val;
        }

        public void Append(ref string target, string val)
        {
            target += _openingDI + val + _closingDI;
        }

        public string Join(string delimiter, params string[] vals)
        {
            var result = string.Empty;
            for (int i = 0; i < vals.Length; i++)
            {
                Wrap(ref vals[i]);
                result += vals[i];

                if (i != vals.Length - 1)
                    result += delimiter;
            }
            return result;
        }

        private void SetDIs(SqlStyle style)
        {
            switch (style)
            {
                case SqlStyle.MySQL:
                    _openingDI = _closingDI = '`';
                    break;

                case SqlStyle.MSSQL:
                    _openingDI = '[';
                    _closingDI = ']';
                    break;

                default:
                    _openingDI = _closingDI = '\"';
                    break;
            }
        }
    }
}
