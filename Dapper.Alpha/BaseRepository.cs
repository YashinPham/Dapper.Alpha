using Dapper.Alpha.Extensions;
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
        public DbSession DbSession;

        protected ISqlBuilder _SqlBuilder => DbSession.SqlBuilder;

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
            throw new NotImplementedException();
        }

        public Task<int> BulkInsertAsync(IEnumerable<TEntity> instances)
        {
            throw new NotImplementedException();
        }

        public bool Delete(TEntity instance, int? commandTimeout = null)
        {
            var cmd = _SqlBuilder.GetCmdDeleteById<TEntity>();
            var dynParms = new DynamicParameters();
            var logicalDeletes = cmd.LogicalDeletedParams.ToArray();

            for (int i = 0; i < logicalDeletes.Count(); i++)
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, logicalDeletes[i].StatusProperty.Name, i), logicalDeletes[i].DeleteValue);

            foreach (var prop in cmd.SqlParams)
                dynParms.Add(_SqlBuilder.SqlOptions.ParameterPrefix + prop.Name, prop.GetValue(instance));

            return DbSession.Connection.Execute(cmd.ToString(), dynParms, DbSession.Transaction, commandTimeout: commandTimeout) > 0;
        }

        public Task<int> DeleteAsync(TEntity instance, int? commandTimeout = null)
        {
            var cmd = _SqlBuilder.GetCmdDeleteById<TEntity>();
            var dynParms = new DynamicParameters();
            var logicalDeletes = cmd.LogicalDeletedParams.ToArray();

            for (int i = 0; i < logicalDeletes.Count(); i++)
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, logicalDeletes[i].StatusProperty.Name, i), logicalDeletes[i].DeleteValue);

            foreach (var prop in cmd.SqlParams)
                dynParms.Add(_SqlBuilder.SqlOptions.ParameterPrefix + prop.Name, prop.GetValue(instance));

            return DbSession.Connection.ExecuteAsync(cmd.ToString(), dynParms, DbSession.Transaction, commandTimeout: commandTimeout);
        }

        public bool Delete(Expression<Func<TEntity, bool>> predicate, int? commandTimeout = null)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdDelete<TEntity>();
            var dynParms = conditions.ToDynamicParameters();
            var logicalDeletes = cmd.LogicalDeletedParams.ToArray();

            for (int i = 0; i < logicalDeletes.Count(); i++)
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, logicalDeletes[i].StatusProperty.Name, i), logicalDeletes[i].DeleteValue);

            if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.Execute(cmd.ToString(), dynParms, DbSession.Transaction, commandTimeout: commandTimeout) > 0;
        }

        public Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate, int? commandTimeout = null)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdDelete<TEntity>();
            var dynParms = conditions.ToDynamicParameters();
            var logicalDeletes = cmd.LogicalDeletedParams.ToArray();

            for (int i = 0; i < logicalDeletes.Count(); i++)
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, logicalDeletes[i].StatusProperty.Name, i), logicalDeletes[i].DeleteValue);

            if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.ExecuteAsync(cmd.ToString(), dynParms, DbSession.Transaction, commandTimeout: commandTimeout);
        }

        public bool Update(TEntity instance)
        {
            var cmd = _SqlBuilder.GetCmdUpdate<TEntity>();
            var sqlParams = _SqlBuilder.GetParams(instance, cmd.SqlParams.Select(o => o.Name));
            return DbSession.Connection.Execute(cmd.ToString(), sqlParams, DbSession.Transaction) > 0;
        }

        public bool Update(TEntity instance, params Expression<Func<TEntity, object>>[] includes)
        {
            var cmd = _SqlBuilder.GetCmdUpdateIncludes<TEntity>(includes);
            var sqlParams = _SqlBuilder.GetParams(instance, cmd.SqlParams.Select(o => o.Name), includes);
            return DbSession.Connection.Execute(cmd.ToString(), sqlParams, DbSession.Transaction) > 0;
        }

        public Task<int> UpdateAsync(TEntity instance)
        {
            var cmd = _SqlBuilder.GetCmdUpdate<TEntity>();
            var sqlParams = _SqlBuilder.GetParams(instance, cmd.SqlParams.Select(o => o.Name));
            return DbSession.Connection.ExecuteAsync(cmd.ToString(), sqlParams, DbSession.Transaction);
        }

        public Task<int> UpdateAsync(TEntity instance, params Expression<Func<TEntity, object>>[] includes)
        {
            var cmd = _SqlBuilder.GetCmdUpdate<TEntity>();
            var sqlParams = _SqlBuilder.GetParams(instance, cmd.SqlParams.Select(o => o.Name), includes);
            return DbSession.Connection.ExecuteAsync(cmd.ToString(), sqlParams, DbSession.Transaction);
        }

        public Task<int> BulkUpdateAsync(IEnumerable<TEntity> instances)
        {
            throw new NotImplementedException();
        }

        public bool BulkUpdate(IEnumerable<TEntity> instances)
        {
            throw new NotImplementedException();
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
                    cmd.SqlCmd.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.QuerySingleOrDefault<int>(cmd.ToString(), dynParms, DbSession.Transaction);
        }

        public int Count(Expression<Func<TEntity, object>> distinctField)
        {
            var cmd = _SqlBuilder.GetCmdCount<TEntity>(distinctField);
            var dynParms = new DynamicParameters();
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            return DbSession.Connection.QuerySingleOrDefault<int>(cmd.ToString(), dynParms, DbSession.Transaction);
        }

        public int Count(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdCount<TEntity>(distinctField);
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
                    cmd.SqlCmd.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.QuerySingleOrDefault<int>(cmd.ToString(), dynParms, DbSession.Transaction);
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
                    cmd.SqlCmd.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.QuerySingleOrDefaultAsync<int>(cmd.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<int> CountAsync(Expression<Func<TEntity, object>> distinctField)
        {
            var cmd = _SqlBuilder.GetCmdCount<TEntity>(distinctField);
            var dynParms = new DynamicParameters();
            int deletedParamIndex = 1;
            foreach (var deletedInfo in cmd.LogicalDeletedParams)
            {
                dynParms.Add(string.Format("{0}{1}_{2}", _SqlBuilder.SqlOptions.ParameterPrefix, deletedInfo.StatusProperty.Name, deletedParamIndex), deletedInfo.DeleteValue);
                deletedParamIndex++;
            }
            return DbSession.Connection.QuerySingleOrDefaultAsync<int>(cmd.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate, Expression<Func<TEntity, object>> distinctField)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdCount<TEntity>(distinctField);
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
                    cmd.SqlCmd.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.QuerySingleOrDefaultAsync<int>(cmd.ToString(), dynParms, DbSession.Transaction);
        }

        public TEntity Find() => Find(null);

        public TEntity Find(Expression<Func<TEntity, bool>> predicate)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdFind<TEntity>();
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
                    cmd.SqlCmd.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.QueryFirstOrDefault<TEntity>(cmd.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<TEntity> FindAsync() => FindAsync(null);

        public Task<TEntity> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            var queryProperties = _SqlBuilder.GetQueryProperties<TEntity>(predicate?.Body);
            var qLevel = 0;
            var sqlBuilder = new StringBuilder();
            var conditions = new List<KeyValuePair<string, object>>();
            _SqlBuilder.BuildQuerySql<TEntity>(queryProperties, ref sqlBuilder, ref conditions, ref qLevel);

            var cmd = _SqlBuilder.GetCmdFind<TEntity>();
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
                    cmd.SqlCmd.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.QueryFirstOrDefaultAsync<TEntity>(cmd.ToString(), dynParms, DbSession.Transaction);
        }

        public TEntity FindById(object id)
        {
            var cmd = _SqlBuilder.GetCmdFindById<TEntity>();
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
            return DbSession.Connection.QueryFirstOrDefault<TEntity>(cmd.ToString(), dynParms, DbSession.Transaction);
        }

        public Task<TEntity> FindByIdAsync(object id)
        {
            var cmd = _SqlBuilder.GetCmdFindById<TEntity>();
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
            return DbSession.Connection.QueryFirstOrDefaultAsync<TEntity>(cmd.ToString(), dynParms, DbSession.Transaction);
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
                    cmd.SqlCmd.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.Query<TEntity>(cmd.ToString(), dynParms, DbSession.Transaction);
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
                    cmd.SqlCmd.AppendFormat(" AND {0}", sqlBuilder);
            }
            else if (sqlBuilder.Length > 0)
                cmd.SqlCmd.AppendFormat(" WHERE {0}", sqlBuilder);

            return DbSession.Connection.QueryAsync<TEntity>(cmd.ToString(), dynParms, DbSession.Transaction);
        }
    }
}
