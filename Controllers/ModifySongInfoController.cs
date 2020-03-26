using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ModifySongInfoController : ControllerBase
    {
        private readonly DbContextApp db;
        public ModifySongInfoController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public void Index([FromForm] int songId, [FromForm] string songName="", [FromForm] string songArtist="")
        {
            Song songToModify = db.Songs.Find(songId);

            if (songName != "")
            {
                songToModify.OriginalFileName = songName;
            }
            if (songArtist != "")
            {
                songToModify.Artist = songArtist;
            }
            db.Songs.Update(songToModify);
            db.SaveChanges();
        }
    }
}