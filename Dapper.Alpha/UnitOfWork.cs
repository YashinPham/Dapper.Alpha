using Dapper.Alpha.Configurations;
using System;
using System.Collections.Concurrent;
using System.Data;

namespace Dapper.Alpha
{
    public class UnitOfWork : IUnitOfWork
    {
        public DbSession DbSession { get; private set; }

        private ConcurrentDictionary<Type, object> _repositories = new ConcurrentDictionary<Type, object>();

        public UnitOfWork()
        {
            DbSession = new DbSession(DbSessiontOptionsBuilder.GetConnection());
        }

        public UnitOfWork(IDbConnection connection)
        {
            DbSession = new DbSession(connection);
        }

        public IRepository<TEntity> GetRepository<TEntity>() where TEntity : class
        {
            var type = typeof(TEntity);
            if (!_repositories.ContainsKey(type))
            {
                _repositories[type] = new BaseRepository<TEntity>(DbSession);
            }
            return (IRepository<TEntity>)_repositories[type];
        }

        public IDbTransaction BeginTransaction()
        {
            return DbSession.BeginTransaction();
        }

        public void Commit()
        {
            DbSession.CommitTransaction();
        }

        public void Rollback()
        {
            DbSession.Rollback();
        }

        public void Dispose()
        {
            DbSession.Dispose();
        }
    }
}
