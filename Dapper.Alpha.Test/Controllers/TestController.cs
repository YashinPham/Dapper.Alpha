using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Dapper.Alpha.Test.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public TestController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet]
        public async Task<IActionResult> GetAsync()
        {
            var entryTypeRepo = _unitOfWork.GetRepository<EntryType>();
            var entry = entryTypeRepo.FindById(100);
            entry.AAUpdatedDate = DateTime.Now;
            entryTypeRepo.Update(entry, p => p.ACEntryTypeDesc, p => p.ACEntryTypeName);
            return Ok(entryTypeRepo.FindById(100));
        }
    }
}
