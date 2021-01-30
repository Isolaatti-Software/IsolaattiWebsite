/*
* Isolaatti project
* Erik Cavazos, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class DeleteSongController : ControllerBase
    {
        private readonly DbContextApp db;
        public DeleteSongController(DbContextApp contextApp)
        {
            db = contextApp;
        }
        // normal delete. includes: deleting song record from db, and deleting the files from server
        [HttpPost]
        public IActionResult Index([FromForm] int songId, [FromForm] int userId, [FromForm] string password)
        {
            try
            {
                User user = db.Users.Find(userId);
                if (user.Password.Equals(password))
                {
                    Song songToDelete = db.Songs.Find(songId);
                    if (!songToDelete.OwnerId.Equals(userId)) return Unauthorized("Song is not yours");
                    // deletes database record of song
                    db.Songs.Remove(songToDelete);
                    db.SaveChanges();
                    //return Ok(songToDelete.Uid);
                    return Ok(new
                    {
                        uid = songToDelete.Uid,
                        urls = new []
                        {
                            songToDelete.BassUrl,
                            songToDelete.DrumsUrl,
                            songToDelete.OtherUrl,
                            songToDelete.VoiceUrl
                        }
                    });
                }
            }
            catch (InvalidOperationException)
            {
                return StatusCode(404);
            }
            return StatusCode(401);
        }
        
        [HttpPost]
        [Route("All")]
        public IActionResult DeleteAll([FromForm] int userId, [FromForm] string password)
        {
            var user = db.Users.Find(userId);
            if (user == null) return NotFound();
            if (!user.Password.Equals(password)) return Unauthorized("Wrong credential");
            var allSongs = db.Songs.Where(song => song.OwnerId.Equals(user.Id));
            db.Songs.RemoveRange(allSongs);
            db.SaveChanges();
            return Ok();
        }
    }
}