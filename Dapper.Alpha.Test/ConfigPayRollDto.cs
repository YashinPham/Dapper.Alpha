using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Alpha.Test
{
    public class ConfigPayRollDto
    {
        [Required]
        public Guid? Id { get; set; }

        [Required]
        public int? LockDateMonthly { get; set; }
    }
}
