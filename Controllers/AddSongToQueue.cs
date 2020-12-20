/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
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
        public void Index([FromForm]string userId, [FromForm] string songName, [FromForm] string url,[FromForm] string songArtist="Unknown")
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