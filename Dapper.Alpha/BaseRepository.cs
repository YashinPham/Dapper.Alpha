using Dapper.Alpha.Extensions;
using Dapper.Alpha.Metadata;
using Dapper.Alpha.SqlBuilders;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Alpha
{
    public class BaseRepository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        protected DbSession DbSession;

        protected ISqlBuilder _SqlBuilder => DbSession.SqlBuilder;

        public IDbConnection Connection => DbSession.Connection;

        public BaseRepository(DbSession session)
        {
            DbSession = session;
        }

        public bool Insert(TEntity instance)
        {
            return _SqlBuilder.Insert<TEntity>(instance, DbSession);
        }

        public TKey Insert<TKey>(TEntity instance)
        {
            return _SqlBuilder.Insert<TKey, TEntity>(instance, DbSession);
        }

        public Task<TKey> InsertAsync<TKey>(TEntity instance)
        {
            return _SqlBuilder.InsertAsync<TKey, TEntity>(instance, DbSession);
        }

        public Task<bool> InsertAsync(TEntity instance)
        {
            return _SqlBuilder.InsertAsync<TEntity>(instance, DbSession);
        }

        public int BulkInsert(IEnumerable<TEntity> instances)
        {
            return _SqlBuilder.BulkInsert<TEntity>(instances, DbSession);
        }

        public Task<int> BulkInsertAsync(IEnumerable<TEntity> instances)
        {
            return _SqlBuilder.BulkInsertAsync<TEntity>(instances, DbSession);
        }

        public bool Delete(TEntity instance, int? commandTimeout = null)
        {
            var cmd = _SqlBuilder.GetCmdDeleteById<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = new DynamicParameters();
            var logicalDeletes = cmd.LogicalDeletedParams.ToArray();

            for (int i = 0; i < logicalDeletes.Count(); i++)
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, logicalDeletes[i].StatusProperty.Name, i), logicalDeletes[i].DeleteValue);

            foreach (var prop in cmd.SqlParams)
                dynParms.Add(_SqlBuilder.SqlOptions.ParameterPrefix + prop.Name, prop.GetValue(instance));

            return Connection.Execute(sqlQuery.ToString(), dynParms, DbSession.Transaction, commandTimeout: commandTimeout) > 0;
        }

        public Task<int> DeleteAsync(TEntity instance, int? commandTimeout = null)
        {
            var cmd = _SqlBuilder.GetCmdDeleteById<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = new DynamicParameters();
            var logicalDeletes = cmd.LogicalDeletedParams.ToArray();

            for (int i = 0; i < logicalDeletes.Count(); i++)
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, logicalDeletes[i].StatusProperty.Name, i), logicalDeletes[i].DeleteValue);

            foreach (var prop in cmd.SqlParams)
                dynParms.Add(_SqlBuilder.SqlOptions.ParameterPrefix + prop.Name, prop.GetValue(instance));

            return Connection.ExecuteAsync(sqlQuery.ToString(), dynParms, DbSession.Transaction, commandTimeout: commandTimeout);
        }

        public bool Delete(Expression<Func<TEntity, bool>> predicate, int? commandTimeout = null)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdDelete<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var logicalDeletes = cmd.LogicalDeletedParams.ToArray();

            for (int i = 0; i < logicalDeletes.Count(); i++)
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, logicalDeletes[i].StatusProperty.Name, i), logicalDeletes[i].DeleteValue);

            if (sqlBuilder.Length > 0)
                sqlQuery.AppendFormat(" WHERE {0}", sqlBuilder);

            return Connection.Execute(sqlQuery.ToString(), dynParms, DbSession.Transaction, commandTimeout: commandTimeout) > 0;
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, int? commandTimeout = null)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdDelete<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var logicalDeletes = cmd.LogicalDeletedParams.ToArray();

            for (int i = 0; i < logicalDeletes.Count(); i++)
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, logicalDeletes[i].StatusProperty.Name, i), logicalDeletes[i].DeleteValue);

            if (sqlBuilder.Length > 0)
                sqlQuery.AppendFormat(" WHERE {0}", sqlBuilder);

            return Connection.ExecuteAsync(sqlQuery.ToString(), dynParms, DbSession.Transaction, commandTimeout: commandTimeout);
        }

        public bool Update(TEntity instance)
        {
            var cmd = _SqlBuilder.GetCmdUpdate<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var sqlParams = _SqlBuilder.GetParams(instance, cmd.SqlParams.Select(o => o.Name));
            return Connection.Execute(sqlQuery.ToString(), sqlParams, DbSession.Transaction) > 0;
        }

        public bool Update(TEntity instance, params Expression<Func<TEntity, object>>[] includes)
        {
            var cmd = _SqlBuilder.GetCmdUpdateIncludes<TEntity>(includes);
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var sqlParams = _SqlBuilder.GetParams(instance, cmd.SqlParams.Select(o => o.Name), includes);
            return Connection.Execute(sqlQuery.ToString(), sqlParams, DbSession.Transaction) > 0;
        }

        public Task<int> UpdateAsync(TEntity instance)
        {
            var cmd = _SqlBuilder.GetCmdUpdate<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var sqlParams = _SqlBuilder.GetParams(instance, cmd.SqlParams.Select(o => o.Name));
            return Connection.ExecuteAsync(sqlQuery.ToString(), sqlParams, DbSession.Transaction);
        }

        public Task<int> UpdateAsync(TEntity instance, params Expression<Func<TEntity, object>>[] includes)
        {
            var cmd = _SqlBuilder.GetCmdUpdate<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var sqlParams = _SqlBuilder.GetParams(instance, cmd.SqlParams.Select(o => o.Name), includes);
            return Connection.ExecuteAsync(sqlQuery.ToString(), sqlParams, DbSession.Transaction);
        }

        public Task<int> BulkUpdateAsync(IEnumerable<TEntity> instances)
        {
            var entityType = typeof(TEntity);
            var cmd = _SqlBuilder.GetCmdUpdate<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var multiExec = _SqlBuilder.GetParams<TEntity>(instances, cmd.SqlParams.Select(o => o.Name));
            return Connection.ExecuteAsync(sqlQuery.ToString(), multiExec, transaction: DbSession.Transaction);
        }

        public bool BulkUpdate(IEnumerable<TEntity> instances)
        {
            var entityType = typeof(TEntity);
            var cmd = _SqlBuilder.GetCmdUpdate<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var multiExec = _SqlBuilder.GetParams<TEntity>(instances, cmd.SqlParams.Select(o => o.Name));
            return Connection.Execute(sqlQuery.ToString(), multiExec, transaction: DbSession.Transaction) > 0;
        }

        public int Count() => Count(null);

        public int Count(Expression<Func<TEntity, bool>> predicate)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdCount<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var hasDeleted = false;
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                hasDeleted = true;
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            if (hasDeleted)
            {
                if (sqlBuilder.Length > 0)
                    sqlQuery.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                sqlQuery.AppendFormat(" WHERE {0}", sqlBuilder);

            return Connection.QuerySingleOrDefault<int>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public int Count(Expression<Func<TEntity, object>> distinctField)
        {
            var cmd = _SqlBuilder.GetCmdCount<TEntity>(distinctField);
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = new DynamicParameters();
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            return Connection.QuerySingleOrDefault<int>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public int Count(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdCount<TEntity>(distinctField);
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var hasDeleted = false;
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                hasDeleted = true;
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            if (hasDeleted)
            {
                if (sqlBuilder.Length > 0)
                    sqlQuery.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                sqlQuery.AppendFormat(" WHERE {0}", sqlBuilder);

            return Connection.QuerySingleOrDefault<int>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<int> CountAsync() => CountAsync(null);

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdCount<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var hasDeleted = false;
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                hasDeleted = true;
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            if (hasDeleted)
            {
                if (sqlBuilder.Length > 0)
                    sqlQuery.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                sqlQuery.AppendFormat(" WHERE {0}", sqlBuilder);

            return Connection.QuerySingleOrDefaultAsync<int>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<int> CountAsync(Expression<Func<TEntity, object>> distinctField)
        {
            var cmd = _SqlBuilder.GetCmdCount<TEntity>(distinctField);
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = new DynamicParameters();
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            return Connection.QuerySingleOrDefaultAsync<int>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdCount<TEntity>(distinctField);
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var hasDeleted = false;
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                hasDeleted = true;
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            if (hasDeleted)
            {
                if (sqlBuilder.Length > 0)
                    sqlQuery.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                sqlQuery.AppendFormat(" WHERE {0}", sqlBuilder);

            return Connection.QuerySingleOrDefaultAsync<int>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public TEntity Find() => Find(null);

        public TEntity Find(Expression<Func<TEntity, bool>> predicate)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdFind<TEntity>(true);
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var hasDeleted = false;
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                hasDeleted = true;
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            if (hasDeleted)
            {
                if (sqlBuilder.Length > 0)
                    sqlQuery.Replace(SqlParameterTemplate.WhereClause, string.Format(" AND {0}", sqlBuilder));
            }
            else if (sqlBuilder.Length > 0)
                sqlQuery.Replace(SqlParameterTemplate.WhereClause, string.Format(" WHERE {0}", sqlBuilder));

            return Connection.QueryFirstOrDefault<TEntity>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<TEntity> FindAsync() => FindAsync(null);

        public Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdFind<TEntity>(true);
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var hasDeleted = false;
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                hasDeleted = true;
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            if (hasDeleted)
            {
                if (sqlBuilder.Length > 0)
                    sqlQuery.Replace(SqlParameterTemplate.WhereClause, string.Format(" AND {0}", sqlBuilder));
            }
            else if (sqlBuilder.Length > 0)
                sqlQuery.Replace(SqlParameterTemplate.WhereClause, string.Format(" WHERE {0}", sqlBuilder));

            return Connection.QueryFirstOrDefaultAsync<TEntity>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public TEntity FindById(object id)
        {
            var cmd = _SqlBuilder.GetCmdFindById<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var propsParams = cmd.SqlParams;
            var dynParms = new DynamicParameters();
            if (propsParams.Count() == 1)
                dynParms.Add(_SqlBuilder.SqlOptions.ParameterPrefix + propsParams.First().Name, id);
            else
            {
                foreach (var prop in propsParams)
                    dynParms.Add(_SqlBuilder.SqlOptions.ParameterPrefix + prop.Name, id.GetType().GetProperty(prop.Name).GetValue(id, null));
            }
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            return Connection.QueryFirstOrDefault<TEntity>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<TEntity> FindByIdAsync(object id)
        {
            var cmd = _SqlBuilder.GetCmdFindById<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var propsParams = cmd.SqlParams;
            var dynParms = new DynamicParameters();
            if (propsParams.Count() == 1)
                dynParms.Add(_SqlBuilder.SqlOptions.ParameterPrefix + propsParams.First().Name, id);
            else
            {
                foreach (var prop in propsParams)
                    dynParms.Add(_SqlBuilder.SqlOptions.ParameterPrefix + prop.Name, id.GetType().GetProperty(prop.Name).GetValue(id, null));
            }
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            return Connection.QueryFirstOrDefaultAsync<TEntity>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public IEnumerable<TEntity> FindAll() => FindAll(null);

        public IEnumerable<TEntity> FindAll(Expression<Func<TEntity, bool>> predicate)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdFindAll<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var hasDeleted = false;
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                hasDeleted = true;
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            if (hasDeleted)
            {
                if (sqlBuilder.Length > 0)
                    sqlQuery.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                sqlQuery.AppendFormat(" WHERE {0}", sqlBuilder);

            return Connection.Query<TEntity>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<IEnumerable<TEntity>> FindAllAsync() => FindAllAsync(null);

        public Task<IEnumerable<TEntity>> FindAllAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdFindAll<TEntity>();
            var sqlQuery = new StringBuilder(cmd.SqlCmd);
            var dynParms = conditions.ToDynamicParameters();
            var hasDeleted = false;
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                hasDeleted = true;
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            if (hasDeleted)
            {
                if (sqlBuilder.Length > 0)
                    sqlQuery.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                sqlQuery.AppendFormat(" WHERE {0}", sqlBuilder);

            return Connection.QueryAsync<TEntity>(sqlQuery.ToString(), dynParms, DbSession.Transaction);
        }
    }
}
