using Dapper.Alpha.Attributes;
using System;
using System.Reflection;

namespace Dapper.Alpha.Metadata
{
    public class SqlPropertyInfo
    {
        public SqlPropertyInfo(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            var columnAttr = PropertyInfo.GetCustomAttribute<ColumnAttribute>();
            if (!string.IsNullOrEmpty(columnAttr?.Name))
                SqlColumnName = ColumnAttrName = columnAttr.Name;
            else
                SqlColumnName = PropertyInfo.Name;

            IsPrimaryKey = PropertyInfo.GetCustomAttribute<KeyAttribute>() != null;
            IsComputed = PropertyInfo.GetCustomAttribute<ComputedAttribute>() != null;
            IsIdentity = PropertyInfo.GetCustomAttribute<IdentityAttribute>() != null;
            IsEnumDbString = PropertyInfo.GetCustomAttribute<StatusAttribute>()?.IsEnumDbString ?? false;
            IsNullable = propertyInfo.PropertyType.IsGenericType && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>);
        }

        public PropertyInfo PropertyInfo { get; }

        public bool IsPrimaryKey { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsComputed { get; set; }

        public string ColumnAttrName { get; set; }

        public string SqlColumnName { get; set; }

        public bool IsNullable { get; set; }

        public bool IsEnumDbString { get; set; }

        public virtual string PropertyName => PropertyInfo.Name;
    }
}
