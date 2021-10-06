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
            var config = await ConfigPayRollRepository.FindAsync();
            if (config == null)
            {
                config = new ConfigPayRoll();
                config.Id = Guid.NewGuid();
                config.CreatedAt = DateTime.Now;
                await ConfigPayRollRepository.InsertAsync(config);
            }
            return _mapper.Map<ConfigPayRollDto>(config);
        }


        [HttpPut]
        public async Task<bool> UpdateAsync(ConfigPayRollDto dto)
        {
            var config = await ConfigPayRollRepository.FindByIdAsync(dto.Id);
            config = _mapper.Map(dto, config);
            config.ModifiedAt = DateTime.Now;
            return await ConfigPayRollRepository.UpdateAsync(config) > 0;
        }

        [HttpPost]
        public async Task<bool> InsertAsync(ConfigPayRollDto dto)
        {
            var entryRepo = _unitOfWork.GetRepository<EntryType>();
            var config = new EntryType();
            config.AACreatedDate = DateTime.Now;
            config.AAStatus = ObjectStatus.Alive;
            config.ACEntryTypeName = "Test";
            config.ACEntryTypeDesc = "";
            var id  = entryRepo.Insert<long>(config);
            return true;
        }
    }
}
