using Dapper.Alpha.Configurations;
using Dapper.Alpha.Metadata;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Dapper.Alpha.Infrastructure
{
    public class DbConnectionFactory : IDbConnectionFactory
    {
        private string _connectionString { get; set; }

        private SqlDialect _sqlDialect { get; set; }

        public DbConnectionFactory(string connectionString, SqlDialect sqlDialect)
        {
            _connectionString = connectionString;
            _sqlDialect = sqlDialect;
        }

        public IDbConnection CreateConnection()
        {
            return CreateDbConnection(_connectionString, _sqlDialect);
        }

        private IDbConnection CreateDbConnection(string connectionString, SqlDialect dialect)
        {
            DbConnection connection = null;
            OrmConfiguration.Dialect = dialect;
            if (connectionString != null)
            {
                DbProviderFactory factory = DbProviderFactoryUtils.GetDbProviderFactory(dialect);
                connection = factory.CreateConnection();
                connection.ConnectionString = connectionString;
            }
            return connection;
        }


    }
}
