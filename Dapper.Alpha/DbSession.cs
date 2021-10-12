using Dapper.Alpha.Configurations;
using Dapper.Alpha.Metadata;
using Dapper.Alpha.SqlBuilders;
using System;
using System.Data;

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
            switch (OrmConfiguration.Dialect)
            {
                case SqlDialect.MsSql:
                    {
                        Dialect = SqlDialect.MsSql;
                        SqlBuilder = MsSqlBuilder.GetInstance();
                        break;
                    }
                case SqlDialect.MySql:
                    {
                        Dialect = SqlDialect.MsSql;
                        SqlBuilder = MySqlBuilder.GetInstance();
                        break;
                    }
                case SqlDialect.PostgreSql:
                    {
                        Dialect = SqlDialect.PostgreSql;
                        SqlBuilder = PostreSqlBuilder.GetInstance();
                        break;
                    }
                case SqlDialect.SqLite:
                    {
                        Dialect = SqlDialect.SqLite;
                        SqlBuilder = SqliteBuilder.GetInstance();
                        break;
                    }
                default:
                    throw new DataException($"Dapper.Alpha does not supports sql provider { OrmConfiguration.Dialect}");
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
