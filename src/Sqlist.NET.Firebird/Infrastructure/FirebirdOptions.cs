namespace Sqlist.NET.Infrastructure
{
    public class FirebirdOptions : DbOptions
    {
        public Action<string>? ConfigureDataSource { get; set; }
    }
}
