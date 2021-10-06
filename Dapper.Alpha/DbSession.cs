using Dapper.Alpha.Metadata;
using Dapper.Alpha.SqlBuilders;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Data.SqlClient;

namespace Dapper.Alpha
{
    public class DbSession : IDisposable
    {
        private readonly Guid Id = Guid.NewGuid();

        private IDbConnection InnerConnection { get; set; }

        public IDbConnection Connection => GetConnection();

        public IDbTransaction Transaction { get; private set; }

        public ISqlBuilder SqlBuilder { get; private set; }

        public SqlDialect Dialect { get; private set; }

        public DbSession(IDbConnection connection)
        {
            InnerConnection = connection;
            InitSqlBuilder();
        }

        public void InitSqlBuilder()
        {
            if (InnerConnection is SqlConnection)
            {
                SqlBuilder = MsSqlBuilder.GetInstance();
                Dialect = SqlDialect.MsSql;
            }
            else if (InnerConnection is MySqlConnection)
            {
                SqlBuilder = MySqlBuilder.GetInstance();
                Dialect = SqlDialect.MySql;
            }
            else
            {
                throw new DataException($"Dapper.Alpha only supports {SqlDialect.MySql}, {SqlDialect.MsSql}");
            }
        }

        private IDbConnection GetConnection()
        {
            OpenConnection();
            return InnerConnection;
        }

        public IDbTransaction BeginTransaction()
        {
            return Transaction = Connection.BeginTransaction();
        }

        public void CommitTransaction()
        {
            Transaction?.Commit();
            Transaction = null;
        }

        public void Rollback()
        {
            Transaction?.Rollback();
            Transaction = null;
        }

        public void OpenConnection()
        {
            var wasClosed = InnerConnection.State == ConnectionState.Closed;
            if (wasClosed) InnerConnection.Open();
        }

        public void Dispose()
        {
            Transaction?.Dispose();
            Connection?.Dispose();
        }
    }
}
