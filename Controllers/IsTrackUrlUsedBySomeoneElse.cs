using System.Linq;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class IsTrackUrlUsedBySomeoneElse : Controller
    {
        private readonly DbContextApp db;
        public IsTrackUrlUsedBySomeoneElse(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        
        [HttpPost]
        public bool Index([FromForm] string url,[FromForm] int userWhoAsksId)
        {
            // TODO: Improve this as only sees if 1 or more tracks of the 4 are used.
            return db.Songs.Any(element => 
                !element.OwnerId.Equals(userWhoAsksId) &&
                (element.BassUrl.Equals(url) || element.DrumsUrl.Equals(url) 
                                             || element.VoiceUrl.Equals(url) || element.OtherUrl.Equals(url)));
        }
    }
}