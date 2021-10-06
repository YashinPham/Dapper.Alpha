using System;

namespace Dapper.Alpha.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class StatusAttribute : Attribute
    {
        public StatusAttribute()
        {
        }

        public bool IsEnumDbString { get; set; }
    }
}
