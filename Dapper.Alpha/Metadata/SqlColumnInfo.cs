using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Alpha.Metadata
{
    public class SqlColumnInfo
    {
        public string Database { get; set; }

        public string Owner { get; set; }

        public string TableName { get; set; }

        public string ColumnName { get; set; }

        public int OrdinalPosition { get; set; }

        public string DefaultSetting { get; set; }

        public string DataType { get; set; }

        public int? MaxLength { get; set; }

        public int? DatePrecision { get; set; }

        public int? NumberPrecision { get; set; }

        public int? NumberScale { get; set; }

        public bool IsNullable { get; set; }

        public bool IsIdentity { get; set; }

        public bool IsComputed { get; set; }

        public bool IsPrimaryKey { get; set; }

        public string DbType => GetDbType();

        private string GetDbType()
        {
            if ((MaxLength ?? 0) > 0 && (DataType?.ToLower() == "nvarchar" || DataType?.ToLower() == "varchar"))
                return string.Format("{0}({1})", DataType, MaxLength);
            else if (DataType?.ToLower() == "decimal")
                return string.Format("{0}({1},{2})", DataType, NumberPrecision ?? 18, NumberScale ?? 0);

            return DataType;
        }
    }
}
