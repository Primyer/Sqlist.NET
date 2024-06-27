using System.Text;

namespace Sqlist.NET.Metadata
{
    public class CopyOptions
    {
        public string? Format { get; set; }
        public bool Freeze { get; set; }
        public string? Delimiter { get; set; }
        public string? Null { get; set; }
        public string? Header { get; set; }
        public string? Quote { get; set; }
        public string? Escape { get; set; }
        public string? ForceQuote { get; set; }
        public string? ForceNotNull { get; set; }
        public string? ForceNull { get; set; }
        public string? Encoding { get; set; }

        public override string ToString()
        {
            var sb = new StringBuilder();

            Append(sb, "FORMAT", Format);

            if (Freeze)
                sb.Append("FREEZE");

            Append(sb, "DELIMITER", Delimiter, true);
            Append(sb, "NULL", Null, true);
            Append(sb, "HEADER", Header);
            Append(sb, "QUOTE", Quote, true);
            Append(sb, "FORCE_QUOTE", ForceQuote, true);
            Append(sb, "FORCE_NOT_NULL", ForceNotNull);
            Append(sb, "FORCE_NULL", ForceNull);
            Append(sb, "ENCODING", Encoding, true);

            return sb.ToString();
        }

        private static void Append(StringBuilder sb, string name, string? value, bool quote = false)
        {
            if (value is null)
                return;

            if (sb.Length != 0)
                sb.AppendLine(",");

            sb.Append(name + " ");
            sb.Append(quote ? $"'{value}'" : value);
        }
    }
}
