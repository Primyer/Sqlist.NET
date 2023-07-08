namespace Sqlist.NET.Infrastructure
{
    public class NpgsqlOptionsBuilder : DbOptionsBuilder
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="NpgsqlOptionsBuilder"/> class.
        /// </summary>
        public NpgsqlOptionsBuilder(NpgsqlOptions options) : base(options)
        {
        }
    }
}
