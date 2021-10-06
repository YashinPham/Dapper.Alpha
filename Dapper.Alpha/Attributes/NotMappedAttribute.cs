using System;

namespace Dapper.Alpha.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class NotMappedAttribute : Attribute
    {
    }
}
