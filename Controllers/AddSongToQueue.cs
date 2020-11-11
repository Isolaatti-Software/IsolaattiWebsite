using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AddSongToQueue : Controller
    {
        private readonly DbContextApp Db;
        public AddSongToQueue(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        [HttpPost]
        public void Index([FromForm]string userId, [FromForm] string songName, [FromForm] string songArtist, [FromForm] string url)
        {
            SongQueue songQueue = new SongQueue()
            {
                AudioSourceUrl = url,
                Reserved = false,
                SongName = songName,
                SongArtist = songArtist,
                UserId = userId
            };

            Db.SongsQueue.Add(songQueue);
            Db.SaveChanges();
        }
    }
}