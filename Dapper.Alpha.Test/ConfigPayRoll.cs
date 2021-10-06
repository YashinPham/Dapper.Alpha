using Dapper.Alpha.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Alpha.Test
{
    [Table("ConfigPayRolls", Schema = "common")]
    public class ConfigPayRoll
    {
        [Key]
        public Guid Id { get; set; }

        [Status, Deleted]
        public bool IsDeleted { get; set; }

        public DateTime? CreatedAt { get; set; }

        public Guid? CreatedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }

        public Guid? ModifiedBy { get; set; }

        public int? LockDateMonthly { get; set; }
    }
}
