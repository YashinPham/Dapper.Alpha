using System;
using System.Data;

namespace Dapper.Alpha
{
    public interface IUnitOfWork : IDisposable
    {
        DbSession DbSession { get; }

        IRepository<TEntity> GetRepository<TEntity>() where TEntity : class;

        IDbTransaction BeginTransaction();

        void Commit();

        void Rollback();
    }
}
