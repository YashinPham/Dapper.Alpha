using System.Data;
using System.Reflection;

namespace Dapper.Alpha.Metadata
{
    public class LogicalDeleteInfo
    {
        public PropertyInfo StatusProperty { get; set; }

        public string PropertyName => StatusProperty?.Name;

        public string SqlColumnName { get; set; }

        public object DeleteValue { get; set; }

        public DbType DbType { get; set; }

        public int Order { get; set; }
    }
}
