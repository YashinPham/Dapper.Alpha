using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Alpha.Configurations
{
    public class SQLiteDatabaseOptions : SqlDatabaseOptions
    {
        public SQLiteDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = "`";
            this.SqlIdentityStatement = "SELECT LAST_INSERT_ROWID() AS id";
            this.SqlQueryLimitStatement = "SELECT {SelectColumns} FROM {TableName} {WhereClause} LIMIT {LimitRows}";
            this.SqlPagingStatement = "SELECT {SelectColumns} FROM {TableName} {WhereClause} ORDER BY {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
        }
    }
}
