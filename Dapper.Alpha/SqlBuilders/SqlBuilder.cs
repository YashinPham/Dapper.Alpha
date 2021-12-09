using Dapper.Alpha.Attributes;
using Dapper.Alpha.Configurations;
using Dapper.Alpha.Extensions;
using Dapper.Alpha.Metadata;
using Dapper.Alpha.SqlBuilders.QueryExpressions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Alpha.SqlBuilders
{
    public class SqlBuilder : ISqlBuilder
    {
        private static readonly ConcurrentDictionary<Type, string> TableNames = new ConcurrentDictionary<Type, string>();

        private static readonly ConcurrentDictionary<Type, IEnumerable<SqlPropertyInfo>> SqlProperties = new ConcurrentDictionary<Type, IEnumerable<SqlPropertyInfo>>();

        private static readonly ConcurrentDictionary<string, SqlCmdQuery> SqlBuilderCacheDict = new ConcurrentDictionary<string, SqlCmdQuery>();

        private static readonly ConcurrentDictionary<Type, IEnumerable<LogicalDeleteInfo>> LogicalDeleteCacheDict = new ConcurrentDictionary<Type, IEnumerable<LogicalDeleteInfo>>();

        private readonly SqlDialect Dialect;

        public SqlDatabaseOptions SqlOptions { get; private set; }

        public SqlBuilder(SqlDatabaseOptions options)
        {
            SqlOptions = options;
            if (SqlOptions is MsSqlDatabaseOptions)
                Dialect = SqlDialect.MsSql;
            else if (SqlOptions is MySqlDatabaseOptions)
                Dialect = SqlDialect.MySql;
            else if (SqlOptions is PostreSqlDatabaseOptions)
                Dialect = SqlDialect.PostgreSql;
            else if (SqlOptions is SQLiteDatabaseOptions)
                Dialect = SqlDialect.SqLite;
        }

        #region Common Func

        protected string GetTableName(Type entityType)
        {
            return TableNames.GetOrAdd(entityType, entityType =>
            {
                TableAttribute attr = entityType.GetCustomAttributes(false).OfType<TableAttribute>().FirstOrDefault();
                if (attr != null)
                    return SqlOptions.IsUsingSchemas
                        ? string.Format(SqlOptions.Delimiter, attr.Schema) + "." + string.Format(SqlOptions.Delimiter, attr.Name)
                        : string.Format(SqlOptions.Delimiter, attr.Name);

                return SqlOptions.IsUsingSchemas
                    ? string.Format(SqlOptions.Delimiter, SqlOptions.DefaultSchema) + "." + string.Format(SqlOptions.Delimiter, string.Format("{0}s", entityType.Name))
                    : string.Format(SqlOptions.Delimiter, string.Format("{0}s", entityType.Name));
            });
        }

        protected IEnumerable<SqlPropertyInfo> GetSqlProperties(Type entityType)
        {
            return SqlProperties.GetOrAdd(entityType, entityType =>
            {
                var sqlProps = entityType.GetProperties().Where(p => p.GetCustomAttribute<NotMappedAttribute>() == null);
                return sqlProps.Where(ExpressionHelper.GetPrimitivePropertiesPredicate()).Select(o => new SqlPropertyInfo(o));
            });
        }

        protected IEnumerable<LogicalDeleteInfo> GetLogicalDeleteInfo(Type entityType)
        {
            return LogicalDeleteCacheDict.GetOrAdd(entityType, entityType =>
            {
                var deletedInfos = new List<LogicalDeleteInfo>();
                var sqlProps = GetSqlProperties(entityType);
                var statusProp = sqlProps.FirstOrDefault(p => p.PropertyInfo.GetCustomAttributes(true).Any(attr => attr.GetType().Name == typeof(StatusAttribute).Name));
                if (statusProp == null)
                    return deletedInfos;

                if (statusProp.PropertyInfo.PropertyType == typeof(bool))
                {
                    deletedInfos.Add(new LogicalDeleteInfo()
                    {
                        StatusProperty = statusProp.PropertyInfo,
                        DeleteValue = true,
                        DbType = DbType.Boolean,
                        SqlColumnName = statusProp.SqlColumnName
                    });
                }
                else if (statusProp.PropertyInfo.PropertyType.IsEnum)
                {
                    var deletedProps = statusProp.PropertyInfo.PropertyType.GetFields().Where(f => f.GetCustomAttribute<DeletedAttribute>() != null).ToList();
                    foreach (var deletedProp in deletedProps)
                    {
                        var deletedAttr = (DeletedAttribute)deletedProp.GetCustomAttributes(true).FirstOrDefault(attr => attr.GetType().Name == typeof(DeletedAttribute).Name);
                        var enumValue = Enum.Parse(statusProp.PropertyInfo.PropertyType, deletedProp.Name);
                        if (statusProp.IsEnumDbString)
                        {
                            deletedInfos.Add(new LogicalDeleteInfo()
                            {
                                StatusProperty = statusProp.PropertyInfo,
                                DeleteValue = enumValue.ToString(),
                                DbType = DbType.String,
                                Order = deletedAttr.Order,
                                SqlColumnName = statusProp.SqlColumnName
                            });
                        }
                        else
                        {
                            deletedInfos.Add(new LogicalDeleteInfo()
                            {
                                StatusProperty = statusProp.PropertyInfo,
                                DeleteValue = enumValue,
                                DbType = DbType.Int32,
                                Order = deletedAttr.Order,
                                SqlColumnName = statusProp.SqlColumnName
                            });
                        }
                    }
                }
                return deletedInfos;
            });
        }

        protected string BuildSelectParameters(Type entityType)
        {
            var cacheKey = $"{Dialect}_{entityType}_BuildSelectParameters";
            var sqlCmd = SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var sbColumns = new StringBuilder();
                var sqlProps = GetSqlProperties(entityType).ToList();
                for (var i = 0; i < sqlProps.Count(); i++)
                {
                    var sqlProp = sqlProps[i];
                    if (sqlProp.ColumnAttrName.IsNullOrWhiteSpace())
                        sbColumns.AppendFormat(SqlOptions.Delimiter, sqlProp.PropertyName);
                    else
                    {
                        sbColumns.AppendFormat(SqlOptions.Delimiter, sqlProp.SqlColumnName);
                        sbColumns.AppendFormat(" AS ");
                        sbColumns.AppendFormat(SqlOptions.Delimiter, sqlProp.PropertyName);
                    }
                    if (i < sqlProps.Count() - 1)
                        sbColumns.Append(", ");
                }
                return new SqlCmdQuery(sbColumns);
            });
            return sqlCmd.ToString();
        }

        protected string BuildUpdateParameters(Type entityType)
        {
            var cacheKey = $"{Dialect}_{entityType}_BuildUpdateParameters";
            var sqlCmd = SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var updateProps = GetSqlProperties(entityType).Where(o => !o.IsPrimaryKey).ToList();
                var sbColumns = new StringBuilder();
                for (var i = 0; i < updateProps.Count; i++)
                {
                    var property = updateProps[i];
                    sbColumns.AppendFormat("{0} = {1}{2}", string.Format(SqlOptions.Delimiter, property.SqlColumnName), SqlOptions.ParameterPrefix, property.PropertyName);
                    if (i < updateProps.Count - 1)
                        sbColumns.Append(", ");
                }
                return new SqlCmdQuery(sbColumns);
            });
            return sqlCmd.ToString();
        }

        public void BuildQuerySql<TEntity>(IList<QueryExpression> queryProperties, ref StringBuilder sqlBuilder, ref List<KeyValuePair<string, object>> conditions, ref int qLevel)
        {
            var entityType = typeof(TEntity);
            var sqlProps = GetSqlProperties(entityType);
            foreach (var expr in queryProperties)
            {
                if (!string.IsNullOrEmpty(expr.LinkingOperator))
                {
                    if (sqlBuilder.Length > 0)
                        sqlBuilder.Append(" ");

                    sqlBuilder
                        .Append(expr.LinkingOperator)
                        .Append(" ");
                }
                switch (expr)
                {
                    case QueryParameterExpression qpExpr:
                        var tableName = GetTableName(entityType);
                        string columnName = sqlProps.First(x => x.PropertyName == qpExpr.PropertyName).SqlColumnName;
                        if (qpExpr.PropertyValue == null)
                        {
                            sqlBuilder.AppendFormat("{0}.{1} {2} NULL", tableName, columnName, qpExpr.QueryOperator == "=" ? "IS" : "IS NOT");
                        }
                        else
                        {
                            var vKey = string.Format("{0}_p{1}", qpExpr.PropertyName, qLevel); //Handle multiple uses of a field

                            sqlBuilder.AppendFormat("{0}.{1} {2} {3}{4}", tableName, columnName, qpExpr.QueryOperator, SqlOptions.ParameterPrefix, vKey);
                            conditions.Add(new KeyValuePair<string, object>(vKey, qpExpr.PropertyValue));
                        }
                        qLevel++;
                        break;
                    case QueryBinaryExpression qbExpr:
                        var nSqlBuilder = new StringBuilder();
                        var nConditions = new List<KeyValuePair<string, object>>();
                        BuildQuerySql<TEntity>(qbExpr.Nodes, ref nSqlBuilder, ref nConditions, ref qLevel);

                        if (qbExpr.Nodes.Count == 1) //Handle `grouping brackets`
                            sqlBuilder.Append(nSqlBuilder);
                        else
                            sqlBuilder.AppendFormat("({0})", nSqlBuilder);

                        conditions.AddRange(nConditions);
                        break;
                }
            }
        }

        public List<QueryExpression> GetQueryProperties<TEntity>(Expression expr)
        {
            if (expr == null)
                return new List<QueryExpression>();

            var queryNode = GetQueryProperties<TEntity>(expr, ExpressionType.Default);
            switch (queryNode)
            {
                case QueryParameterExpression qpExpr:
                    return new List<QueryExpression> { queryNode };

                case QueryBinaryExpression qbExpr:
                    return qbExpr.Nodes;

                default:
                    throw new NotSupportedException(queryNode.ToString());
            }
        }

        private QueryExpression GetQueryProperties<TEntity>(Expression expr, ExpressionType linkingType)
        {
            var entityType = typeof(TEntity);
            var isNotUnary = false;
            if (expr is UnaryExpression unaryExpression)
            {
                if (unaryExpression.NodeType == ExpressionType.Not && unaryExpression.Operand is MethodCallExpression)
                {
                    expr = unaryExpression.Operand;
                    isNotUnary = true;
                }
            }
            else if (expr is BinaryExpression binaryExpression)
            {
                var sqlProps = GetSqlProperties(entityType);
                if (binaryExpression.NodeType != ExpressionType.AndAlso && binaryExpression.NodeType != ExpressionType.OrElse)
                {
                    var propertyName = ExpressionHelper.GetPropertyNamePath(binaryExpression, out var isNested);
                    bool checkNullable = isNested && propertyName.EndsWith("HasValue");

                    if (!checkNullable)
                    {
                        if (!sqlProps.Select(x => x.PropertyName).Contains(propertyName))
                            throw new NotSupportedException("predicate can't parse");
                    }
                    else
                    {
                        var prop = sqlProps.FirstOrDefault(x => x.IsNullable && x.PropertyName + "HasValue" == propertyName);
                        if (prop == null)
                        {
                            prop = sqlProps.FirstOrDefault(x => x.IsNullable && x.PropertyName + "HasValue" == propertyName);
                            if (prop == null)
                                throw new NotSupportedException("predicate can't parse");
                        }
                        else
                        {
                            isNested = false;
                        }
                        propertyName = prop.PropertyName;
                    }
                    var propertyValue = ExpressionHelper.GetValue(binaryExpression.Right);
                    var nodeType = checkNullable ? ((bool)propertyValue == false ? ExpressionType.Equal : ExpressionType.NotEqual) : binaryExpression.NodeType;
                    if (checkNullable)
                    {
                        propertyValue = null;
                    }
                    var opr = ExpressionHelper.GetSqlOperator(nodeType);
                    var link = ExpressionHelper.GetSqlOperator(linkingType);

                    return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
                }

                var leftExpr = GetQueryProperties<TEntity>(binaryExpression.Left, ExpressionType.Default);
                var rightExpr = GetQueryProperties<TEntity>(binaryExpression.Right, binaryExpression.NodeType);

                switch (leftExpr)
                {
                    case QueryParameterExpression lQPExpr:
                        if (!string.IsNullOrEmpty(lQPExpr.LinkingOperator) && !string.IsNullOrEmpty(rightExpr.LinkingOperator)) // AND a AND B
                        {
                            switch (rightExpr)
                            {
                                case QueryBinaryExpression rQBExpr:
                                    if (lQPExpr.LinkingOperator == rQBExpr.Nodes.Last().LinkingOperator) // AND a AND (c AND d)
                                    {
                                        var nodes = new QueryBinaryExpression
                                        {
                                            LinkingOperator = leftExpr.LinkingOperator,
                                            Nodes = new List<QueryExpression> { leftExpr }
                                        };

                                        rQBExpr.Nodes[0].LinkingOperator = rQBExpr.LinkingOperator;
                                        nodes.Nodes.AddRange(rQBExpr.Nodes);

                                        leftExpr = nodes;
                                        rightExpr = null;
                                        // AND a AND (c AND d) => (AND a AND c AND d)
                                    }
                                    break;
                            }
                        }
                        break;

                    case QueryBinaryExpression lQBExpr:
                        switch (rightExpr)
                        {
                            case QueryParameterExpression rQPExpr:
                                if (rQPExpr.LinkingOperator == lQBExpr.Nodes.Last().LinkingOperator) //(a AND b) AND c
                                {
                                    lQBExpr.Nodes.Add(rQPExpr);
                                    rightExpr = null;
                                    //(a AND b) AND c => (a AND b AND c)
                                }
                                break;
                            case QueryBinaryExpression rQBExpr:
                                if (lQBExpr.Nodes.Last().LinkingOperator == rQBExpr.LinkingOperator) // (a AND b) AND (c AND d)
                                {
                                    if (rQBExpr.LinkingOperator == rQBExpr.Nodes.Last().LinkingOperator) // AND (c AND d)
                                    {
                                        rQBExpr.Nodes[0].LinkingOperator = rQBExpr.LinkingOperator;
                                        lQBExpr.Nodes.AddRange(rQBExpr.Nodes);
                                        // (a AND b) AND (c AND d) =>  (a AND b AND c AND d)
                                    }
                                    else
                                    {
                                        lQBExpr.Nodes.Add(rQBExpr);
                                        // (a AND b) AND (c OR d) =>  (a AND b AND (c OR d))
                                    }
                                    rightExpr = null;
                                }
                                break;
                        }
                        break;
                }
                var nLinkingOperator = ExpressionHelper.GetSqlOperator(linkingType);
                if (rightExpr == null)
                {
                    leftExpr.LinkingOperator = nLinkingOperator;
                    return leftExpr;
                }
                return new QueryBinaryExpression
                {
                    NodeType = QueryExpressionType.Binary,
                    LinkingOperator = nLinkingOperator,
                    Nodes = new List<QueryExpression> { leftExpr, rightExpr },
                };
            }
            else if (expr is MethodCallExpression methodCallExpression)
            {
                var methodName = methodCallExpression.Method.Name;
                var exprObj = methodCallExpression.Object;
            MethodLabel:
                switch (methodName)
                {
                    case "Contains":
                        {
                            if (exprObj != null && exprObj.NodeType == ExpressionType.MemberAccess && exprObj.Type == typeof(string))
                            {
                                methodName = "StringContains";
                                goto MethodLabel;
                            }
                            var propertyName = ExpressionHelper.GetPropertyNamePath(methodCallExpression, out var isNested);
                            var sqlProps = GetSqlProperties(entityType);
                            if (!sqlProps.Select(x => x.PropertyName).Contains(propertyName))
                                throw new NotSupportedException("predicate can't parse");

                            var propertyValue = ExpressionHelper.GetValuesFromCollection(methodCallExpression);
                            var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                            var link = ExpressionHelper.GetSqlOperator(linkingType);
                            return new QueryParameterExpression(link, propertyName, propertyValue, opr, isNested);
                        }
                    case "StringContains":
                    case "CompareString":
                    case "Equals":
                    case "StartsWith":
                    case "EndsWith":
                        {
                            if (exprObj == null || exprObj.NodeType != ExpressionType.MemberAccess)
                            {
                                goto default;
                            }
                            var propertyName = ExpressionHelper.GetPropertyNamePath(exprObj, out bool isNested);
                            var sqlProps = GetSqlProperties(entityType);

                            if (!sqlProps.Select(x => x.PropertyName).Contains(propertyName))
                                throw new NotSupportedException("predicate can't parse");

                            var propertyValue = ExpressionHelper.GetValuesFromStringMethod(methodCallExpression);
                            var likeValue = ExpressionHelper.GetSqlLikeValue(methodName, propertyValue);
                            var opr = ExpressionHelper.GetMethodCallSqlOperator(methodName, isNotUnary);
                            var link = ExpressionHelper.GetSqlOperator(linkingType);
                            return new QueryParameterExpression(link, propertyName, likeValue, opr, isNested);
                        }
                    default:
                        throw new NotSupportedException($"'{methodName}' method is not supported");
                }
            }
            return GetQueryProperties<TEntity>(ExpressionHelper.GetBinaryExpression(expr), linkingType);
        }

        #endregion Common Func

        public SqlCmdQuery GetCmdFindById<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cacheKey = $"{Dialect}_{entityType}_BuildCmdFindById";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var sqlProps = GetSqlProperties(entityType);
                var whereProps = sqlProps.Where(prop => prop.IsPrimaryKey).ToList();
                if (!whereProps.Any())
                    throw new ArgumentException("GetFindById<T> only supports an entity with a [KeyAttribute]");

                var tableName = GetTableName(entityType);
                var logicalDeletes = GetLogicalDeleteInfo(entityType).ToList();

                var cmd = new StringBuilder(SqlOptions.SqlQueryStatement);
                cmd.Replace(SqlParameterTemplate.SelectColumns, BuildSelectParameters(entityType));
                cmd.Replace(SqlParameterTemplate.TableName, tableName);

                var whereClause = new StringBuilder();
                var delimiter = SqlOptions.Delimiter;
                var paramsRefix = SqlOptions.ParameterPrefix;
                var aliasName = string.Empty;
                for (var i = 0; i < whereProps.Count; i++)
                {
                    if (i > 0) cmd.Append(" AND ");
                    whereClause.AppendFormat("{0} = {1}{2}", string.Format(delimiter, whereProps[i].SqlColumnName), paramsRefix, whereProps[i].PropertyName);
                }
                int logicalIndixex = 1;
                logicalDeletes.ForEach(logicalDelete =>
                {
                    if (logicalDelete != null && whereClause.Length > 0)
                        whereClause.AppendFormat(" AND {0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);
                    else
                        whereClause.AppendFormat("{0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);

                    logicalIndixex++;
                });

                if (whereClause.Length != 0)
                    whereClause.Insert(0, "WHERE ");

                cmd.Replace(SqlParameterTemplate.WhereClause, whereClause.ToString());
                return new SqlCmdQuery(cmd, whereProps.Select(o => o.PropertyInfo), logicalDeletes);
            });
        }

        public SqlCmdQuery GetCmdFindAll<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cacheKey = $"{Dialect}_{entityType}_FindAll";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var logicalDeletes = GetLogicalDeleteInfo(entityType).ToList();

                var cmd = new StringBuilder(SqlOptions.SqlQueryStatement);
                cmd.Replace(SqlParameterTemplate.SelectColumns, BuildSelectParameters(entityType));
                cmd.Replace(SqlParameterTemplate.TableName, tableName);

                var whereClause = new StringBuilder();
                var delimiter = SqlOptions.Delimiter;
                var paramsRefix = SqlOptions.ParameterPrefix;

                int logicalIndixex = 1;
                logicalDeletes.ForEach(logicalDelete =>
                {
                    if (logicalDelete != null && whereClause.Length > 0)
                        whereClause.AppendFormat(" AND {0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);
                    else
                        whereClause.AppendFormat("{0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);

                    logicalIndixex++;
                });

                if (whereClause.Length != 0)
                    whereClause.Insert(0, "WHERE ");

                cmd.Replace(SqlParameterTemplate.WhereClause, whereClause.ToString());
                return new SqlCmdQuery(cmd, logicalDeletedParams: logicalDeletes);
            });
        }

        public SqlCmdQuery GetCmdInsert<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cacheKey = $"{Dialect}_{entityType}_Insert";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var cmd = new StringBuilder(SqlOptions.SqlInsertStatement);
                var tableName = GetTableName(entityType);
                var sbColumns = new StringBuilder();
                var sqlProps = GetSqlProperties(entityType).ToArray();
                var insertProps = sqlProps.Where(o => !(o.IsIdentity || o.IsComputed)).ToList();
                //var computedProperties = GetComputedProperties(entityType);
                //var insertProps = sqlProps.Except(identityProperties.Union(computedProperties)).ToList();
                for (var i = 0; i < insertProps.Count; i++)
                {
                    var property = insertProps[i];
                    sbColumns.AppendFormat(SqlOptions.Delimiter, property.SqlColumnName);
                    if (i < insertProps.Count - 1)
                        sbColumns.Append(", ");
                }
                var sbParameterList = new StringBuilder(null);
                for (var i = 0; i < insertProps.Count; i++)
                {
                    var property = insertProps[i];
                    sbParameterList.AppendFormat("{0}{1}", SqlOptions.ParameterPrefix, property.PropertyName);
                    if (i < insertProps.Count - 1)
                        sbParameterList.Append(", ");
                }
                cmd.Replace(SqlParameterTemplate.TableName, tableName);
                cmd.Replace(SqlParameterTemplate.InsertColumns, sbColumns.ToString());
                cmd.Replace(SqlParameterTemplate.InsertParameters, sbParameterList.ToString());
                return new SqlCmdQuery(cmd, insertProps.Select(o => o.PropertyInfo));
            });
        }

        public SqlCmdQuery GetCmdCount<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cacheKey = $"{Dialect}_{entityType}_Count";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var logicalDeletes = GetLogicalDeleteInfo(entityType).ToList();
                var cmd = new StringBuilder(SqlOptions.SqlQueryStatement);
                cmd.Replace(SqlParameterTemplate.SelectColumns, "COUNT(1)");
                cmd.Replace(SqlParameterTemplate.TableName, tableName);

                var whereClause = new StringBuilder();
                var delimiter = SqlOptions.Delimiter;
                var paramsRefix = SqlOptions.ParameterPrefix;

                int logicalIndixex = 1;
                logicalDeletes.ForEach(logicalDelete =>
                {
                    if (logicalDelete != null && whereClause.Length > 0)
                        whereClause.AppendFormat(" AND {0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);
                    else
                        whereClause.AppendFormat("{0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);

                    logicalIndixex++;
                });

                if (whereClause.Length != 0)
                    whereClause.Insert(0, "WHERE ");

                cmd.Replace(SqlParameterTemplate.WhereClause, whereClause.ToString());
                return new SqlCmdQuery(cmd, logicalDeletedParams: logicalDeletes);
            });
        }

        public SqlCmdQuery GetCmdFind<TEntity>(bool isAppendWhereClause = false) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cacheKey = $"{Dialect}_{entityType}_Find{(isAppendWhereClause ? "1" : "")}";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var logicalDeletes = GetLogicalDeleteInfo(entityType).ToList();

                var cmd = new StringBuilder(SqlOptions.SqlQueryLimitStatement);
                cmd.Replace(SqlParameterTemplate.LimitRows, string.Format("{0}", 1));
                cmd.Replace(SqlParameterTemplate.SelectColumns, BuildSelectParameters(entityType));
                cmd.Replace(SqlParameterTemplate.TableName, tableName);

                var whereClause = new StringBuilder();
                var delimiter = SqlOptions.Delimiter;
                var paramsRefix = SqlOptions.ParameterPrefix;
                int logicalIndixex = 1;
                logicalDeletes.ForEach(logicalDelete =>
                {
                    if (logicalDelete != null && whereClause.Length > 0)
                        whereClause.AppendFormat(" AND {0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);
                    else
                        whereClause.AppendFormat("{0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);

                    logicalIndixex++;
                });
                if (whereClause.Length != 0)
                    whereClause.Insert(0, "WHERE ");

                cmd.Replace(SqlParameterTemplate.WhereClause, isAppendWhereClause ? String.Format("{0} {1}", whereClause.ToString(), SqlParameterTemplate.WhereClause) : whereClause.ToString());
                return new SqlCmdQuery(cmd, logicalDeletedParams: logicalDeletes);
            });
        }

        public SqlCmdQuery GetCmdCount<TEntity>(Expression<Func<TEntity, object>> distinctField) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var propertyName = ExpressionHelper.GetPropertyName(distinctField);
            var distinctProp = GetSqlProperties(entityType).FirstOrDefault(o => o.PropertyName == propertyName);
            var cacheKey = $"{Dialect}_{entityType}_Count_{propertyName}";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var logicalDeletes = GetLogicalDeleteInfo(entityType).ToList();
                var cmd = new StringBuilder(SqlOptions.SqlQueryStatement);
                cmd.Replace(SqlParameterTemplate.SelectColumns, string.Format("COUNT(DISTINCT {0})", distinctProp.SqlColumnName ?? distinctProp.PropertyName));
                cmd.Replace(SqlParameterTemplate.TableName, tableName);

                var whereClause = new StringBuilder();
                var delimiter = SqlOptions.Delimiter;
                var paramsRefix = SqlOptions.ParameterPrefix;

                int logicalIndixex = 1;
                logicalDeletes.ForEach(logicalDelete =>
                {
                    if (logicalDelete != null && whereClause.Length > 0)
                        whereClause.AppendFormat(" AND {0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);
                    else
                        whereClause.AppendFormat("{0} != {1}{2}_{3}", string.Format(delimiter, logicalDelete.StatusProperty.Name), paramsRefix, logicalDelete.StatusProperty.Name, logicalIndixex);

                    logicalIndixex++;
                });

                if (whereClause.Length != 0)
                    whereClause.Insert(0, "WHERE ");

                cmd.Replace(SqlParameterTemplate.WhereClause, whereClause.ToString());
                return new SqlCmdQuery(cmd, logicalDeletedParams: logicalDeletes);
            });
        }

        public SqlCmdQuery GetCmdDelete<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cacheKey = $"{Dialect}_{entityType}_Delete";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var sqlProps = GetSqlProperties(entityType);

                var logicalDeletes = GetLogicalDeleteInfo(entityType).ToList();
                if (logicalDeletes?.Any() ?? false)
                {
                    var deletedProp = logicalDeletes.OrderBy(o => o.Order).FirstOrDefault();
                    var sqlQuery = new StringBuilder();
                    sqlQuery.Append("UPDATE ")
                            .Append(tableName)
                            .Append(" SET ")
                            .Append(deletedProp.StatusProperty.Name)
                            .Append(" = ")
                            .AppendFormat("{0}{1}_1", SqlOptions.ParameterPrefix, deletedProp.StatusProperty.Name);

                    return new SqlCmdQuery(sqlQuery, logicalDeletedParams: new LogicalDeleteInfo[] { deletedProp });
                }
                else
                {
                    var sqlQuery = new StringBuilder();
                    sqlQuery.Append("DELETE FROM ")
                            .Append(tableName);

                    return new SqlCmdQuery(sqlQuery);
                }
            });
        }

        public SqlCmdQuery GetCmdDeleteById<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cacheKey = $"{Dialect}_{entityType}_Delete";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var sqlProps = GetSqlProperties(entityType);
                var keyProps = sqlProps.Where(prop => prop.IsPrimaryKey).ToList();
                if (!(keyProps?.Any() ?? false))
                    throw new ArgumentException("Entity must have at least one [Key] property");

                var whereClause = new StringBuilder();
                for (var i = 0; i < keyProps.Count(); i++)
                {
                    if (i > 0) whereClause.Append(" AND ");
                    var property = sqlProps.FirstOrDefault(s => s.PropertyName == keyProps[0].PropertyName);
                    whereClause.AppendFormat("{0} = {1}{2}", string.Format(SqlOptions.Delimiter, property.SqlColumnName), SqlOptions.ParameterPrefix, property.SqlColumnName);
                }
                var logicalDeletes = GetLogicalDeleteInfo(entityType).ToList();
                if (logicalDeletes?.Any() ?? false)
                {
                    var deletedProp = logicalDeletes.OrderBy(o => o.Order).FirstOrDefault();
                    var cmd = new StringBuilder(SqlOptions.SqlInsertStatement);
                    cmd.Replace(SqlParameterTemplate.TableName, tableName);
                    cmd.Replace(SqlParameterTemplate.UpdateParameters, string.Format("{0} = {1}{2}_1", deletedProp.StatusProperty.Name, SqlOptions.ParameterPrefix, deletedProp.StatusProperty.Name));
                    cmd.Replace(SqlParameterTemplate.WhereClause, " WHERE " + whereClause);
                    return new SqlCmdQuery(cmd, keyProps.Select(o => o.PropertyInfo), logicalDeletedParams: new LogicalDeleteInfo[] { deletedProp });
                }
                else
                {
                    var sqlQuery = new StringBuilder();
                    sqlQuery.Append("DELETE FROM ")
                            .Append(tableName)
                            .Append(" WHERE ")
                            .Append(whereClause);
                    return new SqlCmdQuery(sqlQuery, keyProps.Select(o => o.PropertyInfo));
                }
            });
        }

        public SqlCmdQuery GetCmdUpdate<TEntity>() where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cacheKey = $"{Dialect}_{entityType}_Update";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var sqlProps = GetSqlProperties(entityType);
                var keyProps = sqlProps.Where(prop => prop.IsPrimaryKey).ToList();
                if (!(keyProps?.Any() ?? false))
                    throw new ArgumentException("Entity must have at least one [Key] property");

                var cmd = new StringBuilder(SqlOptions.SqlUpdateStatement);
                cmd.Replace(SqlParameterTemplate.TableName, tableName);
                cmd.Replace(SqlParameterTemplate.UpdateParameters, BuildUpdateParameters(entityType));
                var whereClause = new StringBuilder();
                for (var i = 0; i < keyProps.Count(); i++)
                {
                    if (i > 0) whereClause.Append(" AND ");
                    var property = sqlProps.FirstOrDefault(s => s.PropertyName == keyProps[0].PropertyName);
                    whereClause.AppendFormat("{0} = {1}{2}", string.Format(SqlOptions.Delimiter, property.SqlColumnName), SqlOptions.ParameterPrefix, property.SqlColumnName);
                }
                cmd.Replace(SqlParameterTemplate.WhereClause, " WHERE " + whereClause);
                return new SqlCmdQuery(cmd, sqlProps.Select(o => o.PropertyInfo));
            });
        }

        public SqlCmdQuery GetCmdUpdateIncludes<TEntity>(params Expression<Func<TEntity, object>>[] includes) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var includeNames = includes.Select(include => ExpressionHelper.GetPropertyName(include)).ToArray();
            var cacheKey = $"{Dialect}_{entityType}_Update_{string.Join("_", includeNames)}";
            return SqlBuilderCacheDict.GetOrAdd(cacheKey, cacheKey =>
            {
                var tableName = GetTableName(entityType);
                var sqlProps = GetSqlProperties(entityType);
                var keyProps = sqlProps.Where(prop => prop.IsPrimaryKey).ToList();
                if (!(keyProps?.Any() ?? false))
                    throw new ArgumentException("Entity must have at least one [Key] property");

                var cmd = new StringBuilder(SqlOptions.SqlUpdateStatement);
                cmd.Replace(SqlParameterTemplate.TableName, tableName);
                var updateParams = new StringBuilder();
                var updateProps = GetSqlProperties(entityType).Where(o => includeNames.Contains(o.PropertyName)).ToList();
                for (var i = 0; i < updateProps.Count; i++)
                {
                    var property = updateProps[i];
                    updateParams.AppendFormat("{0} = {1}{2}", string.Format(SqlOptions.Delimiter, property.SqlColumnName), SqlOptions.ParameterPrefix, property.PropertyName);
                    if (i < updateProps.Count - 1)
                        updateParams.Append(", ");
                }
                cmd.Replace(SqlParameterTemplate.UpdateParameters, updateParams.ToString());
                var whereClause = new StringBuilder();
                for (var i = 0; i < keyProps.Count(); i++)
                {
                    if (i > 0) whereClause.Append(" AND ");
                    var property = sqlProps.FirstOrDefault(s => s.PropertyName == keyProps[0].PropertyName);
                    whereClause.AppendFormat("{0} = {1}{2}", string.Format(SqlOptions.Delimiter, property.SqlColumnName), SqlOptions.ParameterPrefix, property.SqlColumnName);
                }
                cmd.Replace(SqlParameterTemplate.WhereClause, " WHERE " + whereClause);
                return new SqlCmdQuery(cmd, keyProps.Select(o => o.PropertyInfo));
            });
        }

        public DynamicParameters GetParams<TEntity>(TEntity instance, params Expression<Func<TEntity, object>>[] includes) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var includeNames = includes.Select(include => ExpressionHelper.GetPropertyName(include)).ToArray();
            var updateProps = GetSqlProperties(entityType).Where(p => includeNames.Contains(p.PropertyName)).ToArray();
            DynamicParameters dynParams = new DynamicParameters();
            foreach (var property in updateProps)
            {
                if (property.IsEnumDbString)
                    dynParams.Add(string.Format("{0}{1}", SqlOptions.ParameterPrefix, property.PropertyName), property.PropertyInfo.GetValue(instance)?.ToString());
                else
                    dynParams.Add(string.Format("{0}{1}", SqlOptions.ParameterPrefix, property.PropertyName), property.PropertyInfo.GetValue(instance));
            }
            return dynParams;
        }

        public DynamicParameters GetParams<TEntity>(TEntity instance, IEnumerable<string> paramNames, params Expression<Func<TEntity, object>>[] includes) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var sqlProps = GetSqlProperties(entityType);
            var updateParams = paramNames.ToArray();
            if (includes.Length > 0)
            {
                var includeNames = includes.Select(include => ExpressionHelper.GetPropertyName(include)).ToArray();
                updateParams = paramNames.Union(includeNames).ToArray();
            }
            DynamicParameters dynParams = new DynamicParameters();
            foreach (var paramName in updateParams)
            {
                var property = sqlProps.FirstOrDefault(o => o.PropertyName == paramName);
                if (property != null)
                {
                    if (property.IsEnumDbString)
                        dynParams.Add(string.Format("{0}{1}", SqlOptions.ParameterPrefix, property.PropertyName), property.PropertyInfo.GetValue(instance)?.ToString());
                    else
                        dynParams.Add(string.Format("{0}{1}", SqlOptions.ParameterPrefix, property.PropertyName), property.PropertyInfo.GetValue(instance));
                }
            }
            return dynParams;
        }

        public IEnumerable<DynamicParameters> GetParams<TEntity>(IEnumerable<TEntity> instances, IEnumerable<string> paramNames, params Expression<Func<TEntity, object>>[] includes) where TEntity : class
        {
            var multiExec = new List<DynamicParameters>();
            instances.All(instance =>
            {
                var dynParams = GetParams<TEntity>(instance, paramNames, includes);
                multiExec.Add(dynParams);
                return true;
            });
            return multiExec;
        }

        public virtual bool Insert<TEntity>(TEntity instance, DbSession dbSession, int? commandTimeout = null) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cmd = GetCmdInsert<TEntity>();
            var sqlQuery = new StringBuilder(cmd.ToString());
            var sqlProps = GetSqlProperties(entityType);
            var keyProps = sqlProps.Where(prop => prop.IsPrimaryKey).ToList();
            var isIdentity = sqlProps.Any(o => o.IsIdentity);
            if (isIdentity)
                sqlQuery.AppendFormat("; {0}", SqlOptions.SqlIdentityStatement);

            var dynParams = GetParams<TEntity>(instance, sqlProps.Select(o => o.PropertyName));
            var result = dbSession.Connection.QueryMultiple(sqlQuery.ToString(), dynParams, dbSession.Transaction, commandTimeout: commandTimeout);
            var idProperty = keyProps.Select(prop => prop.PropertyInfo).FirstOrDefault();
            if (isIdentity)
            {
                var first = result.Read().FirstOrDefault();
                var id = (int)first.id;
                idProperty.SetValue(instance, Convert.ChangeType(first.id, idProperty.PropertyType), null);
            }
            return true;
        }

        public virtual TKey Insert<TKey, TEntity>(TEntity instance, DbSession dbSession, int? commandTimeout = null) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cmd = GetCmdInsert<TEntity>();
            var sqlQuery = new StringBuilder(cmd.ToString());
            var sqlProps = GetSqlProperties(entityType);
            var keyProps = sqlProps.Where(prop => prop.IsPrimaryKey).ToList();
            var isIdentity = sqlProps.Any(o => o.IsIdentity);
            if (isIdentity)
                sqlQuery.AppendFormat("; {0}", SqlOptions.SqlIdentityStatement);

            var dynParams = GetParams<TEntity>(instance, sqlProps.Select(o => o.PropertyName));
            var result = dbSession.Connection.QueryMultiple(sqlQuery.ToString(), dynParams, dbSession.Transaction, commandTimeout: commandTimeout);
            var idProperty = keyProps.Select(prop => prop.PropertyInfo).FirstOrDefault();
            if (isIdentity)
            {
                var first = result.Read().FirstOrDefault();
                var id = (int)first.id;
                idProperty.SetValue(instance, Convert.ChangeType(first.id, idProperty.PropertyType), null);
            }
            return (TKey)Convert.ChangeType(idProperty.GetValue(instance), typeof(TKey));
        }

        public virtual async Task<bool> InsertAsync<TEntity>(TEntity instance, DbSession dbSession, int? commandTimeout = null) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cmd = GetCmdInsert<TEntity>();
            var sqlQuery = new StringBuilder(cmd.ToString());
            var sqlProps = GetSqlProperties(entityType);
            var keyProps = sqlProps.Where(prop => prop.IsPrimaryKey).ToList();
            var isIdentity = sqlProps.Any(o => o.IsIdentity);
            if (isIdentity)
                sqlQuery.AppendFormat("; {0}", SqlOptions.SqlIdentityStatement);

            var dynParams = GetParams<TEntity>(instance, sqlProps.Select(o => o.PropertyName));
            var result = dbSession.Connection.QueryMultiple(sqlQuery.ToString(), dynParams, dbSession.Transaction, commandTimeout: commandTimeout);
            var idProperty = keyProps.Select(prop => prop.PropertyInfo).FirstOrDefault();
            if (isIdentity)
            {
                var first = result.Read().FirstOrDefault();
                var id = (int)first.id;
                idProperty.SetValue(instance, Convert.ChangeType(first.id, idProperty.PropertyType), null);
            }
            return true;
        }

        public virtual async Task<TKey> InsertAsync<TKey, TEntity>(TEntity instance, DbSession dbSession, int? commandTimeout = null) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cmd = GetCmdInsert<TEntity>();
            var sqlQuery = new StringBuilder(cmd.ToString());
            var sqlProps = GetSqlProperties(entityType);
            var keyProps = sqlProps.Where(prop => prop.IsPrimaryKey).ToList();
            var isIdentity = sqlProps.Any(o => o.IsIdentity);
            if (isIdentity)
                sqlQuery.AppendFormat("; {0}", SqlOptions.SqlIdentityStatement);

            var dynParams = GetParams<TEntity>(instance, sqlProps.Select(o => o.PropertyName));
            var result = await dbSession.Connection.QueryMultipleAsync(sqlQuery.ToString(), dynParams, dbSession.Transaction, commandTimeout: commandTimeout);
            var idProperty = keyProps.Select(prop => prop.PropertyInfo).FirstOrDefault();
            if (isIdentity)
            {
                var first = result.Read().FirstOrDefault();
                var id = (int)first.id;
                idProperty.SetValue(instance, Convert.ChangeType(first.id, idProperty.PropertyType), null);
            }
            return (TKey)Convert.ChangeType(idProperty.GetValue(instance), typeof(TKey));
        }

        public virtual int BulkInsert<TEntity>(IEnumerable<TEntity> instances, DbSession dbSession, int? commandTimeout) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cmd = GetCmdInsert<TEntity>();
            var multiExec = new List<DynamicParameters>();
            var sqlProps = GetSqlProperties(entityType);
            instances?.All(entity =>
            {
                var dynParams = GetParams<TEntity>(entity, sqlProps.Select(o => o.PropertyName));
                multiExec.Add(dynParams);
                return true;
            });
            return dbSession.Connection.Execute(cmd.ToString(), multiExec, dbSession.Transaction, commandTimeout);
        }

        public Task<int> BulkInsertAsync<TEntity>(IEnumerable<TEntity> instances, DbSession dbSession, int? commandTimeout = null) where TEntity : class
        {
            var entityType = typeof(TEntity);
            var cmd = GetCmdInsert<TEntity>();
            var multiExec = new List<DynamicParameters>();
            var sqlProps = GetSqlProperties(entityType);
            instances?.All(entity =>
            {
                var dynParams = GetParams<TEntity>(entity, sqlProps.Select(o => o.PropertyName));
                multiExec.Add(dynParams);
                return true;
            });
            return dbSession.Connection.ExecuteAsync(cmd.ToString(), multiExec, dbSession.Transaction, commandTimeout);
        }
    }
}
