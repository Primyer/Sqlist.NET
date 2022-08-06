#region License
// Copyright (c) 2021, Saleh Kawaf Kulla
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

namespace Sqlist.NET.Utilities
{
    public class Encloser
    {
        private SqlStyle _style = default;

        private bool _enclose;
        private char _di; // Delimited identifier.

        public Encloser(SqlStyle style)
        {
            Style = style;
        }

        public SqlStyle Style
        {
            get => _style;
            set
            {
                SetDI(value);
                _style = value;
            }
        }

        public string Wrap(string val)
        {
            if (!_enclose)
                return val;

            return _di + val + _di;
        }

        public void Wrap(ref string val)
        {
            if (!_enclose)
                return;

            val = _di + val + _di;
        }

        public void Replace(ref string val)
        {
            if (!_enclose)
                return;

            val = val.Replace('`', _di);
        }

        public string Replace(string val)
        {
            if (!_enclose)
                return val;

            return val.Replace('`', _di);
        }

        public void Reformat(ref string val)
        {
            if (val.IndexOf("`") != -1)
                Replace(ref val);
            else
                Wrap(ref val);
        }

        public string Reformat(string val)
        {
            Reformat(ref val);
            return val;
        }

        public string Join(string delimiter, params string[] vals)
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

        private void SetDI(SqlStyle style)
        {
            switch (style)
            {
                case SqlStyle.None:
                    _enclose = false;
                    break;

                default:
                    _enclose = true;
                    _di = '\"';
                    break;
            }
        }
    }
}
