namespace Sqlist.NET.Metadata
{
    public class CopySource
    {
        private readonly string? _content;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CopySource"/> class.
        /// </summary>
        private CopySource(string content) => _content = content;

        public static CopySource StdIn => new("STDIN");
        public static CopySource StdOut => new("STDOUT");

        public static CopySource File(string file)
        {
            return new CopySource($"'{file}'");
        }

        public static CopySource Program(string command)
        {
            return new CopySource($"PROGRAM '{command}'");
        }

        public override string ToString()
        {
            return _content ?? string.Empty;
        }
    }
}
