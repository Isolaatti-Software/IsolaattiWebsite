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
        public IActionResult Index(int userId, string password, int songId, string name, string downloadUrl)
        {
            var user = Db.Users.Find(userId);
            if (user == null) return Unauthorized("User does not exist");
            if (!user.Password.Equals(password)) return Unauthorized("Wrong password");
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