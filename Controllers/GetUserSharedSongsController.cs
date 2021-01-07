using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
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
                from song in _db.Songs
                join share in _db.SharedSongs on song.OwnerId equals share.userId
                where share.userId == userId
                select new 
                {
                    Name = song.OriginalFileName,
                    Artist = song.Artist,
                    Uid = share.uid,
                    Url = $"https://preview.isolaatti.com/publicAPI/Shared?uid={share.uid}"
                };

            return Ok(shares);
        }
    }
}