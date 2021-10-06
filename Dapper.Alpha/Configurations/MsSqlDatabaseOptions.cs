namespace Dapper.Alpha.Configurations
{
    internal class MsSqlDatabaseOptions : SqlDatabaseOptions
    {
        public MsSqlDatabaseOptions()
        {
            this.StartDelimiter = "[";
            this.EndDelimiter = "]";
            this.IsUsingSchemas = true;
            this.SqlIdentityStatement = "SELECT SCOPE_IDENTITY() AS [id]";
            this.SqlQueryLimitStatement = "SELECT TOP {LimitRows} {SelectColumns} FROM {TableName} {WhereClause}";
            this.SqlPagingStatement = "SELECT * FROM (SELECT ROW_NUMBER() OVER(ORDER BY {OrderBy}) AS PagedNumber, {SelectColumns} FROM {TableName} {WhereClause}) AS u WHERE PagedNumber BETWEEN (({PageNumber}-1) * {RowsPerPage} + 1) AND ({PageNumber} * {RowsPerPage})";
        }
    }
}
