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
            if (type == SqlDialect.MsSql)
#if NETFULL
                return GetDbProviderFactory("System.Data.SqlClient.SqlClientFactory", "System.Data.SqlClient");
#else
                return GetDbProviderFactory("Microsoft.Data.SqlClient.SqlClientFactory", "Microsoft.Data.SqlClient");
#endif
            if (type == SqlDialect.SqLite)
            {
#if NETFULL
                return GetDbProviderFactory("System.Data.SQLite.SQLiteFactory", "System.Data.SQLite");
#else
                return GetDbProviderFactory("Microsoft.Data.Sqlite.SqliteFactory", "Microsoft.Data.Sqlite");
#endif
            }
            if (type == SqlDialect.MySql)
                return GetDbProviderFactory("MySql.Data.MySqlClient.MySqlClientFactory", "MySql.Data");
            if (type == SqlDialect.PostgreSql)
                return GetDbProviderFactory("Npgsql.NpgsqlFactory", "Npgsql");

            throw new NotSupportedException(string.Format("Unsupported Provider Factory", type.ToString()));
        }

        public static DbProviderFactory GetDbProviderFactory(string providerName)
        {
#if NETFULL
    return DbProviderFactories.GetFactory(providerName);
#else
            var providername = providerName.ToLower();
            if (providerName == "system.data.sqlclient" || providerName == "microsoft.data.sqlclient")
                return GetDbProviderFactory(SqlDialect.MsSql);
            if (providerName == "system.data.sqlite" || providerName == "microsoft.data.sqlite")
                return GetDbProviderFactory(SqlDialect.SqLite);
            if (providerName == "mysql.data.mysqlclient" || providername == "mysql.data")
                return GetDbProviderFactory(SqlDialect.MySql);
            if (providerName == "npgsql")
                return GetDbProviderFactory(SqlDialect.PostgreSql);

            throw new NotSupportedException(string.Format("Unsupported Provider Factory", providerName));
#endif
        }

        public static DbProviderFactory GetDbProviderFactory(string dbProviderFactoryTypename, string assemblyName)
        {
            var instance = GetStaticProperty(dbProviderFactoryTypename, "Instance");
            if (instance == null)
            {
                var a = LoadAssembly(assemblyName);
                if (a != null)
                    instance = GetStaticProperty(dbProviderFactoryTypename, "Instance");
            }

            if (instance == null)
                throw new InvalidOperationException(string.Format("Unable to retrieve DbProvider Factory Form", dbProviderFactoryTypename));

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
