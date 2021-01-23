/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("api/[controller]")]
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