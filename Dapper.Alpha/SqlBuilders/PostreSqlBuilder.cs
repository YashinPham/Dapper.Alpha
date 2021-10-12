using Dapper.Alpha.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Alpha.SqlBuilders
{

    internal class PostreSqlBuilder : SqlBuilder
    {

        private static PostreSqlBuilder _instance;

        private static object syncLock = new object();

        private PostreSqlBuilder()
            : base(new PostreSqlDatabaseOptions())
        {

        }

        public static PostreSqlBuilder GetInstance()
        {
            if (_instance == null)
            {
                lock (syncLock)
                {
                    if (_instance == null)
                    {
                        _instance = new PostreSqlBuilder();
                    }
                }
            }
            return _instance;
        }
    }
}
