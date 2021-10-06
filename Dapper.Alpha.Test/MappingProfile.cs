using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Alpha.Test
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<ConfigPayRoll, ConfigPayRollDto>();
            CreateMap<ConfigPayRollDto, ConfigPayRoll>();
        }
    }
}
