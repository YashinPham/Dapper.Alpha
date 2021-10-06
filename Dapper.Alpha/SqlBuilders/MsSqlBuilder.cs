using Dapper.Alpha.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Alpha.SqlBuilders
{
    internal class MsSqlBuilder : SqlBuilder
    {
        private static MsSqlBuilder _instance;

        private static object syncLock = new object();

        private MsSqlBuilder()
            : base(new MsSqlDatabaseOptions())
        {

        }

        public static MsSqlBuilder GetInstance()
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new MsSqlBuilder();
                    }
                }
            }
            return _instance;
        }
    }
}
