/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using System.Linq;
using isolaatti_API.isolaatti_lib;
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
        public IActionResult Index([FromForm] string sessionToken, [FromForm] int songId)
        {
            var accountsManager = new Accounts(db);
            var user = accountsManager.ValidateToken(sessionToken);
            if (user == null) return Unauthorized("Token is not valid");
            
            // this means that the same song had already been shared
            if (db.SharedSongs.Any(shares => shares.SharedSongId.Equals(songId)))
            {
                var existingShareUid = db.SharedSongs.Single(sh => sh.SharedSongId.Equals(songId)).uid;
                return Ok($"https://{Request.HttpContext.Request.Host.Value}/public/Shared?uid={existingShareUid}");
            }
            
            var share = new SongShares()
            {
                userId = user.Id,
                SharedSongId = songId,
                uid = Guid.NewGuid().ToString()
            };
                
            // returns uid, client will create the link using this uid
            db.SharedSongs.Add(share);
            db.SaveChanges();
            return Ok($"https://{Request.HttpContext.Request.Host.Value}/publicAPI/Shared?uid={share.uid}");
        }
    }
}