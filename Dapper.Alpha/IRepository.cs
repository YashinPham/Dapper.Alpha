using System;
using System.Collections.Generic;
using System.Data;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Dapper.Alpha
{
    public interface IRepository<TEntity> where TEntity : class
    {
        IDbConnection Connection { get; }

        TKey Insert<TKey>(TEntity instance);

        bool Insert(TEntity instance);

        Task<TKey> InsertAsync<TKey>(TEntity instance);

        Task<bool> InsertAsync(TEntity instance);

        //int BulkInsert(IEnumerable<TEntity> instances);

        //Task<int> BulkInsertAsync(IEnumerable<TEntity> instances);

        bool Delete(TEntity instance, int? commandTimeout = null);

        Task<int> DeleteAsync(TEntity instance, int? commandTimeout = null);

        bool Delete(Expression<Func<TEntity, bool>> predicate, int? commandTimeout = null);

        Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, int? commandTimeout = null);

        bool Update(TEntity instance);

        Task<int> UpdateAsync(TEntity instance);

        bool Update(TEntity instance, params Expression<Func<TEntity, object>>[] includes);

        Task<int> UpdateAsync(TEntity instance, params Expression<Func<TEntity, object>>[] includes);

        //Task<int> BulkUpdateAsync(IEnumerable<TEntity> instances);

        //bool BulkUpdate(IEnumerable<TEntity> instances);

        int Count();

        int Count(Expression<Func<TEntity, bool>> predicate);

        int Count(Expression<Func<TEntity, object>> distinctField);

        int Count(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField);

        Task<int> CountAsync();

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate);

        Task<int> CountAsync(Expression<Func<TEntity, object>> distinctField);

        Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField);

        TEntity Find();

        TEntity Find(Expression<Func<TEntity, bool>> predicate);

        Task<TEntity> FindAsync();

        Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate);

        TEntity FindById(object id);

        Task<TEntity> FindByIdAsync(object id);

        IEnumerable<TEntity> FindAll();

        IEnumerable<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate);

        Task<IEnumerable<TEntity>> FindAllAsync();

        Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate);
    }
}
