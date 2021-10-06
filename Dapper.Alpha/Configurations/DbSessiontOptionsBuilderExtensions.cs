namespace Dapper.Alpha.Configurations
{
    public static class DbSessiontOptionsBuilderExtensions
    {
        public static void UseSqlServer(this DbSessiontOptionsBuilder optionsBuilder, string connectionString)
        {
            DbSessiontOptionsBuilder.Dialect = Metadata.SqlDialect.MsSql;
            DbSessiontOptionsBuilder.ConnectionString = connectionString;
        }

        public static void UseMySql(this DbSessiontOptionsBuilder optionsBuilder, string connectionString)
        {
            DbSessiontOptionsBuilder.Dialect = Metadata.SqlDialect.MySql;
            DbSessiontOptionsBuilder.ConnectionString = connectionString;
        }
    }
}
