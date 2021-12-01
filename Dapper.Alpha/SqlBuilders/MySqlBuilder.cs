using Dapper.Alpha.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Alpha.SqlBuilders
{
    internal class MySqlBuilder : SqlBuilder
    {

        private static MySqlBuilder _instance;

        private static object syncLock = new object();

        private MySqlBuilder()
            : base(new MySqlDatabaseOptions())
        {

        }

        public static MySqlBuilder GetInstance()
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new MySqlBuilder();
                    }
                }
            }
            return _instance;
        }
    }
}
