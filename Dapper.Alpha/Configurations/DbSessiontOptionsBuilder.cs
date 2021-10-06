using Dapper.Alpha.Metadata;
using MySql.Data.MySqlClient;
using System.Data;
using System.Data.SqlClient;

namespace Dapper.Alpha.Configurations
{
    public class DbSessiontOptionsBuilder
    {

        private static DbSessiontOptionsBuilder _instance;

        private static object syncLock = new object();

        private DbSessiontOptionsBuilder()
        {
        }

        public static DbSessiontOptionsBuilder GetInstance()
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new DbSessiontOptionsBuilder();
                    }
                }
            }
            return _instance;
        }

        internal static SqlDialect Dialect { get; set; }

        internal static string ConnectionString { get; set; }

        internal static IDbConnection GetConnection()
        {
            switch (DbSessiontOptionsBuilder.Dialect)
            {
                case SqlDialect.MsSql:
                    return new SqlConnection(ConnectionString);

                case SqlDialect.MySql:
                    return new MySqlConnection(ConnectionString);
                default:
                    throw new DataException($"Dapper.Alpha only supports {SqlDialect.MySql}, {SqlDialect.MsSql}");
            }
        }
    }
}
