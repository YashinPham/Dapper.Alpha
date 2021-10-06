namespace Dapper.Alpha.Configurations
{
    internal class MySqlDatabaseOptions : SqlDatabaseOptions
    {
        public MySqlDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = "`";
            this.SqlIdentityStatement = "SELECT LAST_INSERT_ID() AS id";
            this.SqlQueryLimitStatement = "SELECT {SelectColumns} FROM {TableName} {WhereClause} LIMIT {LimitRows}";
            this.SqlPagingStatement = "SELECT {SelectColumns} FROM {TableName} {WhereClause} ORDER BY {OrderBy} LIMIT {Offset}, {RowsPerPage}";
        }
    }
}
