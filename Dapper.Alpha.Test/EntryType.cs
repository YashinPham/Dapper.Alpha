using Dapper.Alpha.Attributes;
using System;

namespace Dapper.Alpha.Test
{
    [Table("ACEntryTypes", Schema = "dbo")]
    public class EntryType
    {
        [Key, Identity]
        public int ACEntryTypeID { get; set; }

        [Status(IsEnumDbString = true)]
        public ObjectStatus AAStatus { get; set; }

        public DateTime? AACreatedDate { get; set; }

        public string AACreatedUser { get; set; }

        public DateTime? AAUpdatedDate { get; set; }

        public string AAUpdatedUser { get; set; }

        public string ACEntryTypeName { get; set; }

        public string ACEntryTypeDesc { get; set; }
    }
}
