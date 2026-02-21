namespace Sqlist.NET.Infrastructure
{
    public class FirebirdOptionsBuilder(FirebirdOptions options) : DbOptionsBuilder(options)
    {
        private readonly FirebirdOptions _options = options ?? throw new ArgumentNullException(nameof(options));

        public void ConfigureDataSource(Action<string> configure)
        {
            _options.ConfigureDataSource = configure ?? throw new ArgumentNullException(nameof(configure));
        }
    }
}
