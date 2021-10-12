using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Alpha.Configurations
{
    public class PostreSqlDatabaseOptions : SqlDatabaseOptions
    {
        public PostreSqlDatabaseOptions()
        {
            this.StartDelimiter = this.EndDelimiter = "\"";
            this.IsUsingSchemas = true;
            this.DefaultSchema = "public";
            this.SqlIdentityStatement = "SELECT LASTVAL() AS id";
            this.SqlQueryLimitStatement = "SELECT {SelectColumns} FROM {TableName} {WhereClause} LIMIT {LimitRows}";
            this.SqlPagingStatement = "SELECT {SelectColumns} FROM {TableName} {WhereClause} ORDER BY {OrderBy} LIMIT {RowsPerPage} OFFSET (({PageNumber}-1) * {RowsPerPage})";
        }

    }
}
