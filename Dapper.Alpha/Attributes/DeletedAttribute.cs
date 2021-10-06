using System;

namespace Dapper.Alpha.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public sealed class DeletedAttribute : Attribute
    {
        public DeletedAttribute()
        {
        }

        public int Order { get; set; }
    }
}
