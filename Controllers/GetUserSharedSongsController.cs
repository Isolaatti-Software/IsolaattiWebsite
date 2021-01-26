using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class GetUserSharedSongsController : Controller
    {
        private readonly DbContextApp _db;
        public GetUserSharedSongsController(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        [HttpPost]
        public IActionResult Index([FromForm] int userId)
        {
            var shares =
                from share in _db.SharedSongs
                join song in _db.Songs on share.userId equals song.OwnerId
                where share.SharedSongId == song.Id && share.userId == userId
                select new 
                {
                    Name = song.OriginalFileName,
                    Artist = song.Artist,
                    Uid = share.uid,
                    Url = $"https://{Request.HttpContext.Request.Host.Value}/publicAPI/Shared?uid={share.uid}"
                };

            return Ok(shares);
        }
    }
}