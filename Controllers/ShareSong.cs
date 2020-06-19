using System;
using System.Linq;
using isolaatti_API.Classes;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ShareSong : Controller
    {
        private readonly DbContextApp db;

        public ShareSong(DbContextApp _dbContextApp)
        {
            db = _dbContextApp;
        }
        [HttpPost]
        public string Index([FromForm] int userId, [FromForm] string passwd, [FromForm] int songId)
        {
            User userWhoShares = db.Users.Find(userId);
            
            if (userWhoShares.Password == passwd)
            {
                SongShares share = new SongShares()
                {
                    SharedSongId = songId,
                    uid = Guid.NewGuid().ToString()
                };
                // this means that the same song had already been shared
                if (db.SharedSongs.Any(shares => shares.SharedSongId.Equals(songId)))
                {
                    return db.SharedSongs.Single(sh => sh.SharedSongId.Equals(songId)).uid;
                }
                
                // returns uid, client will create the link using this uid
                db.SharedSongs.Add(share);
                db.SaveChanges();
                return share.uid;
            }

            // in case the password is incorrect
            return "err_pwd";
        }
    }
}