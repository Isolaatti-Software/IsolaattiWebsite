/*
* Isolaatti project
* Erik Cavazos, 2020
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
    public class ShareSong : Controller
    {
        private readonly DbContextApp db;

        public ShareSong(DbContextApp _dbContextApp)
        {
            db = _dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm] int userId, [FromForm] string passwd, [FromForm] int songId)
        {
            User userWhoShares = db.Users.Find(userId);
            
            if (userWhoShares.Password == passwd)
            {
                // this means that the same song had already been shared
                if (db.SharedSongs.Any(shares => shares.SharedSongId.Equals(songId)))
                {
                    var existingShareUid = db.SharedSongs.Single(sh => sh.SharedSongId.Equals(songId)).uid;
                    return Ok($"https://{Request.HttpContext.Request.Host.Value}/publicAPI/Shared?uid={existingShareUid}");
                }
                SongShares share = new SongShares()
                {
                    userId = userId,
                    SharedSongId = songId,
                    uid = Guid.NewGuid().ToString()
                };
                
                // returns uid, client will create the link using this uid
                db.SharedSongs.Add(share);
                db.SaveChanges();
                return Ok($"https://{Request.HttpContext.Request.Host.Value}/publicAPI/Shared?uid={share.uid}");
            }

            // in case the password is incorrect
            return Unauthorized("err_pwd");
        }
    }
}