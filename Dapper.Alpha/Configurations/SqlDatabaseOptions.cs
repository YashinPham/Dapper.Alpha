namespace Dapper.Alpha.Configurations
{
    public class SqlDatabaseOptions
    {
        public SqlDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = "\"";
            this.ParameterPrefix = "@";
            this.SqlQueryStatement = "SELECT {SelectColumns} FROM {TableName} {WhereClause}";
            this.SqlInsertStatement = "INSERT INTO {TableName} ({InsertColumns}) values({InsertParameters})";
            this.SqlUpdateStatement = "UPDATE {TableName} SET {UpdateParameters} {WhereClause}";
        }

        public string StartDelimiter { get; protected set; }

        public string EndDelimiter { get; protected set; }

        public bool IsUsingSchemas { get; protected set; }

        public string ParameterPrefix { get; protected set; }

        public string SqlIdentityStatement { get; protected set; }

        public string SqlPagingStatement { get; protected set; }

        public string SqlQueryLimitStatement { get; protected set; }

        public string SqlQueryStatement { get; protected set; }

        public string SqlInsertStatement { get; protected set; }

        public string SqlUpdateStatement { get; protected set; }

        public string Delimiter => StartDelimiter + "{0}" + EndDelimiter;
    }
}
