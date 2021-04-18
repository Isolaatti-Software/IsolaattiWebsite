using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class AddCustomTrack : Controller
    {
        private readonly DbContextApp Db;

        public AddCustomTrack(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken,[FromForm] int songId,[FromForm] string name,
            [FromForm] string downloadUrl)
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var song = Db.Songs.Find(songId);
            if (song == null) return NotFound("Track was not added because song was not found");
            var newCustomTrack = new CustomTrack()
            {
                SongId = songId, DownloadUrl = downloadUrl, Name = name
            };
            Db.CustomTracks.Add(newCustomTrack);
            Db.SaveChanges();
            
            return Ok($"Track {name} added to song {song.OriginalFileName}");
        }
    }
}