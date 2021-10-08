using Dapper.Alpha.Configurations;
using Dapper.Alpha.Metadata;
using Dapper.Alpha.SqlBuilders.QueryExpressions;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Alpha.SqlBuilders
{
    public interface ISqlBuilder
    {
        SqlDatabaseOptions SqlOptions { get; }

        SqlCmdQuery GetCmdCount<TEntity>() where TEntity : class;

        SqlCmdQuery GetCmdCount<TEntity>(Expression<Func<TEntity, object>> distinctField) where TEntity : class;

        SqlCmdQuery GetCmdDeleteById<TEntity>() where TEntity : class;

        SqlCmdQuery GetCmdDelete<TEntity>() where TEntity : class;

        SqlCmdQuery GetCmdFind<TEntity>() where TEntity : class;

        SqlCmdQuery GetCmdFindById<TEntity>() where TEntity : class;

        SqlCmdQuery GetCmdFindAll<TEntity>() where TEntity : class;

        SqlCmdQuery GetCmdInsert<TEntity>() where TEntity : class;

        SqlCmdQuery GetCmdUpdate<TEntity>() where TEntity : class;

        SqlCmdQuery GetCmdUpdateIncludes<TEntity>(params Expression<Func<TEntity, object>>[] includes) where TEntity : class;

        DynamicParameters GetParams<TEntity>(TEntity instance, IEnumerable<string> paramNames, params Expression<Func<TEntity, object>>[] includes) where TEntity : class;

        IEnumerable<DynamicParameters> GetParams<TEntity>(IEnumerable<TEntity> instances, IEnumerable<string> paramNames, params Expression<Func<TEntity, object>>[] includes) where TEntity : class;

        List<QueryExpression> GetQueryProperties<TEntity>(Expression expr);

        void BuildQuerySql<TEntity>(IList<QueryExpression> queryProperties, ref StringBuilder sqlBuilder, ref List<KeyValuePair<string, object>> conditions, ref int qLevel);

        bool Insert<TEntity>(TEntity instance, DbSession dbSession, int? commandTimeout = null) where TEntity : class;

        TKey Insert<TKey, TEntity>(TEntity instance, DbSession dbSession, int? commandTimeout = null) where TEntity : class;

        Task<bool> InsertAsync<TEntity>(TEntity instance, DbSession dbSession, int? commandTimeout = null) where TEntity : class;

        Task<TKey> InsertAsync<TKey, TEntity>(TEntity instance, DbSession dbSession, int? commandTimeout = null) where TEntity : class;

        int BulkInsert<TEntity>(IEnumerable<TEntity> instances, DbSession dbSession, int? commandTimeout = null) where TEntity : class;

        Task<int> BulkInsertAsync<TEntity>(IEnumerable<TEntity> instances, DbSession dbSession, int? commandTimeout = null) where TEntity : class;
    }
}
