using System;

namespace Dapper.Alpha.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class TableAttribute : Attribute
    {
        public TableAttribute(string tableName)
        {
            Name = tableName;
        }

        public string Name { get; set; }

        public string? Schema { get; set; }
    }
}
