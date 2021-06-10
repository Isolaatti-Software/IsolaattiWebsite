/*
* Isolaatti project
* Erik Cavazos, 2021
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/

using System.Linq;
using isolaatti_API.isolaatti_lib;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class DeleteSongController : ControllerBase
    {
        private readonly DbContextApp _db;
        public DeleteSongController(DbContextApp contextApp)
        {
            _db = contextApp;
        }
        // normal delete. includes: deleting song record from db, and deleting the files from server
        [HttpPost]
        public IActionResult Index([FromForm] string sessionToken, [FromForm] int songId)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var songToDelete = _db.Songs.Find(songId);
            if (songToDelete == null) return NotFound($"Song with id {songId} does not exist");
            if (!songToDelete.OwnerId.Equals(user.Id)) return Unauthorized("Song is not yours");
            
            // deletes database record of song
            _db.Songs.Remove(songToDelete);
            
            _db.SaveChanges();
                    
            // urls are returned so that clients use them to see which files should not delete.
            return Ok(new
            {
                uid = songToDelete.Uid,
                // this object should be populated programatically when custom tracks are available
                urls = new
                {
                    bass = songToDelete.BassUrl,
                    drums = songToDelete.DrumsUrl,
                    vocals = songToDelete.VoiceUrl,
                    other = songToDelete.OtherUrl
                }
            });
        }
        
        [HttpPost]
        [Route("All")]
        public IActionResult DeleteAll([FromForm] string sessionToken)
        {
            var accountsManager = new Accounts(_db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            var allSongs = _db.Songs.Where(song => song.OwnerId.Equals(user.Id));
            _db.Songs.RemoveRange(allSongs);
            
            _db.SaveChanges();
            
            return Ok();
        }
    }
}