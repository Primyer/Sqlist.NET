namespace Sqlist.NET
{
    public enum SqlStyle
    {

        /// <summary>
        ///     Represents the SQL syntax that's used in PostgreSQL databases.
        /// </summary>
        PL_pgSQL = 0,

        /// <summary>
        ///     Represents the SQL syntax that's used in Oracle databases.
        /// </summary>
        OracleSQL = 0,

        /// <summary>
        ///     Represents the SQL syntax that's used in Firebird databases.
        /// </summary>
        FirebirdSQL = 0,

        /// <summary>
        ///     Represents the SQL syntax that's used in SQL Server databases.
        /// </summary>
        MSSQL = 1,

        /// <summary>
        ///     Represents the SQL syntax that's used in databases like PHP My Admin.
        /// </summary>
        MySQL = 2
    }
}
