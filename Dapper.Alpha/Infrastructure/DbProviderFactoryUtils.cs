using Dapper.Alpha.Metadata;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Reflection;
using System.Text;

namespace Dapper.Alpha.Infrastructure
{
    internal static class DbProviderFactoryUtils
    {
        public static DbProviderFactory GetDbProviderFactory(SqlDialect type)
        {
            DbProviderFactory factory = null;
            switch (type)
            {
                case SqlDialect.MsSql:
                    {
#if NETFULL
                        factory = GetDbProviderFactory("System.Data.SqlClient.SqlClientFactory", "System.Data.SqlClient");
                        break;
#else
                        factory = GetDbProviderFactory("Microsoft.Data.SqlClient.SqlClientFactory", "Microsoft.Data.SqlClient");
                        if (factory == null)
                            factory = GetDbProviderFactory("System.Data.SqlClient.SqlClientFactory", "System.Data.SqlClient");
                        break;
#endif
                    }
                case SqlDialect.SqLite:
                    {
#if NETFULL
                        factory = GetDbProviderFactory("System.Data.SQLite.SQLiteFactory", "System.Data.SQLite");
                        break;
#else
                        factory = GetDbProviderFactory("Microsoft.Data.Sqlite.SqliteFactory", "Microsoft.Data.Sqlite");
                        if (factory == null)
                            factory = GetDbProviderFactory("System.Data.SQLite.SQLiteFactory", "System.Data.SQLite");

                        break;
#endif
                    }
                case SqlDialect.MySql:
                    factory = GetDbProviderFactory("MySql.Data.MySqlClient.MySqlClientFactory", "MySql.Data");
                    break;

                case SqlDialect.PostgreSql:
                    factory = GetDbProviderFactory("Npgsql.NpgsqlFactory", "Npgsql");
                    break;

            }
            if (factory == null)
                throw new InvalidOperationException($"Could not load library with sql provider of {type} dialect.");

            return factory;
        }

        public static DbProviderFactory GetDbProviderFactory(string dbProviderFactoryTypename, string assemblyName)
        {
            var instance = GetStaticProperty(dbProviderFactoryTypename, "Instance");
            if (instance == null)
            {
                var assembly = LoadAssembly(assemblyName);
                if (assembly != null)
                    instance = GetStaticProperty(dbProviderFactoryTypename, "Instance");
            }
            return instance as DbProviderFactory;
        }

        #region Reflection Utilities
        public static object GetStaticProperty(string typeName, string property)
        {
            Type type = GetTypeFromName(typeName);
            if (type == null)
                return null;

            return GetStaticProperty(type, property);
        }

        public static object GetStaticProperty(Type type, string property)
        {
            object result = null;
            try
            {
                result = type.InvokeMember(property, BindingFlags.Static | BindingFlags.Public | BindingFlags.GetField | BindingFlags.GetProperty, null, type, null);
            }
            catch
            {
                return null;
            }

            return result;
        }

        public static Type GetTypeFromName(string typeName, string assemblyName)
        {
            var type = Type.GetType(typeName, false);
            if (type != null)
                return type;

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in assemblies)
            {
                type = asm.GetType(typeName, false);

                if (type != null)
                    break;
            }
            if (type != null)
                return type;

            if (!string.IsNullOrEmpty(assemblyName))
            {
                var a = LoadAssembly(assemblyName);
                if (a != null)
                {
                    type = Type.GetType(typeName, false);
                    if (type != null)
                        return type;
                }
            }

            return null;
        }

        public static Type GetTypeFromName(string typeName)
        {
            return GetTypeFromName(typeName, null);
        }

        public static Assembly LoadAssembly(string assemblyName)
        {
            Assembly assembly = null;
            try
            {
                assembly = Assembly.Load(assemblyName);
            }
            catch { }

            if (assembly != null)
                return assembly;

            if (File.Exists(assemblyName))
            {
                assembly = Assembly.LoadFrom(assemblyName);
                if (assembly != null)
                    return assembly;
            }
            return null;
        }
        #endregion Reflection Utilities
    }
}
