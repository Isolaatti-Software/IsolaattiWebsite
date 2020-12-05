using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GetUserUsageData : Controller
    {
        private readonly DbContextApp _dbContextApp;

        public GetUserUsageData(DbContextApp dbContextApp)
        {
            _dbContextApp = dbContextApp;
        }
        
        [HttpPost]
        [Route("Percent")]
        public UserUsageStats Index(int id)
        {
            var data = _dbContextApp.UsageData.Single(element => element.UserId.Equals(id));
            return new UserUsageStats(
                data.SoloOnDrums, data.SoloOnBass,
                data.SoloOnVocals, data.SoloOnOther
                );
        }
    }
}