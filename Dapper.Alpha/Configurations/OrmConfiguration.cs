using Dapper.Alpha.Metadata;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Alpha.Configurations
{
    public static class OrmConfiguration
    {
        public static SqlDialect Dialect { get; set; } = SqlDialect.MsSql;
    }
}
