using Dapper.Alpha.Attributes;

namespace Dapper.Alpha.Test
{
    public enum ObjectStatus
    {
        Alive,

        [Deleted(Order = 1)]
        Delete,

        [Deleted(Order = 2)]
        Dummy
    }
}
