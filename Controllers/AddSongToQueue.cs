/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using isolaatti_API.isolaatti_lib;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class AddSongToQueue : Controller
    {
        private readonly DbContextApp Db;
        public AddSongToQueue(DbContextApp dbContextApp)
        {
            Db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm] string songName, [FromForm] string url,[FromForm] string songArtist="Unknown")
        {
            var accountsManager = new Accounts(Db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            SongQueue songQueue = new SongQueue()
            {
                AudioSourceUrl = url,
                Reserved = false,
                SongName = songName,
                SongArtist = songArtist,
                UserId = user.Id.ToString()
            };

            Db.SongsQueue.Add(songQueue);
            Db.SaveChanges();
            return Ok();
        }
    }
}