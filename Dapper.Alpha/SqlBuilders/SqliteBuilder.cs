using Dapper.Alpha.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Alpha.SqlBuilders
{

    internal class SqliteBuilder : SqlBuilder
    {

        private static SqliteBuilder _instance;

        private static object syncLock = new object();

        private SqliteBuilder()
            : base(new SQLiteDatabaseOptions())
        {

        }

        public static SqliteBuilder GetInstance()
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new SqliteBuilder();
                    }
                }
            }
            return _instance;
        }
    }
}
