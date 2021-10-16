using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Dapper.Alpha.Metadata
{
    public class SqlCmdQuery
    {
        public string SqlCmd { get; set; }

        public IEnumerable<LogicalDeleteInfo> LogicalDeletedParams { get; set; }

        public IEnumerable<PropertyInfo> SqlParams { get; set; }

        public SqlCmdQuery(StringBuilder cmd, IEnumerable<PropertyInfo> sqlParams = null, IEnumerable<LogicalDeleteInfo> logicalDeletedParams = null)
        {
            SqlCmd = cmd.ToString();
            SqlParams = sqlParams ?? new PropertyInfo[] { };
            LogicalDeletedParams = logicalDeletedParams ?? new LogicalDeleteInfo[] { };
        }

        public override string ToString()
        {
            return SqlCmd.ToString();
        }
    }
}
