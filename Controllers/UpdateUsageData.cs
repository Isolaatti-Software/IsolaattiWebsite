using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class UpdateUsageData : ControllerBase
    {
        private readonly DbContextApp db;

        public UpdateUsageData(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public void Index([FromForm]int userId, [FromForm]string what)
        {
            if (!db.UsageData.Any(ud => ud.UserId.Equals(userId)))
            {
                var newUsageData = new UserUsageData()
                {
                    SoloOnBass = 0,
                    SoloOnDrums = 0,
                    SoloOnOther = 0,
                    SoloOnVocals = 0,
                    UserId = userId
                };
                switch (what)
                {
                    case "drums": newUsageData.SoloOnDrums += 1;
                        break;
                    case "bass": newUsageData.SoloOnBass += 1;
                        break;
                    case "vocals": newUsageData.SoloOnVocals += 1;
                        break;
                    case "other": newUsageData.SoloOnVocals += 1;
                        break;
                }

                db.UsageData.Add(newUsageData);
                db.SaveChanges();
            }
            else
            {
                var userUsageRecord = db.UsageData.Single(ud => ud.UserId.Equals(userId));
                switch (what)
                {
                    case "drums": userUsageRecord.SoloOnDrums += 1;
                        break;
                    case "bass": userUsageRecord.SoloOnBass += 1;
                        break;
                    case "vocals": userUsageRecord.SoloOnVocals += 1;
                        break;
                    case "other": userUsageRecord.SoloOnOther += 1;
                        break;
                }

                db.UsageData.Update(userUsageRecord);
                db.SaveChanges();
            }
        }
        
    }
}