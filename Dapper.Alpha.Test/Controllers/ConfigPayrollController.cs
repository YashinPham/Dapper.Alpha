using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Alpha.Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConfigPayrollController : ControllerBase
    {

        private readonly IUnitOfWork _unitOfWork;

        private readonly IMapper _mapper;

        private IRepository<ConfigPayRoll> ConfigPayRollRepository => _unitOfWork.GetRepository<ConfigPayRoll>();

        public ConfigPayrollController(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ConfigPayRollDto> GetAsync()
        {
            var id = Guid.Parse("1106872c-e247-40db-8db0-2c0d53fca51c");
            var config = await ConfigPayRollRepository.FindAsync(c=> c.Id == id);
            // if (config == null)
            // {
            //     config = new ConfigPayRoll();
            //     config.Id = Guid.NewGuid();
            //     config.CreatedAt = DateTime.Now;
            //     await ConfigPayRollRepository.InsertAsync(config);
            // }
            return _mapper.Map<ConfigPayRollDto>(config);
        }


        [HttpPut]
        public async Task<bool> UpdateAsync(ConfigPayRollDto dto)
        {
            var configs = await ConfigPayRollRepository.FindAllAsync();
            configs.All(o =>
            {
                o.ModifiedAt = DateTime.Now;
                return true;
            });
            return await ConfigPayRollRepository.BulkUpdateAsync(configs) > 0;
        }

        [HttpPost]
        public async Task<bool> InsertAsync(ConfigPayRollDto dto)
        {
            var entryRepo = _unitOfWork.GetRepository<EntryType>();
            var config = new EntryType();
            config.AACreatedDate = DateTime.Now;
            config.AAStatus = ObjectStatus.Alive;
            config.ACEntryTypeName = "Test 111";
            config.ACEntryTypeDesc = "";

            var list = new List<EntryType>();
            list.Add(new EntryType()
            {
                AACreatedDate = DateTime.Now,
                AAStatus = ObjectStatus.Alive,
                ACEntryTypeName = "Test 111",
                ACEntryTypeDesc = ""
            });
            list.Add(new EntryType()
            {
                AACreatedDate = DateTime.Now,
                AAStatus = ObjectStatus.Alive,
                ACEntryTypeName = "Test 222",
                ACEntryTypeDesc = ""
            });
            var id = entryRepo.BulkInsert(list);
            return true;
        }
    }
}
