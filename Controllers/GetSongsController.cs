using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class GetSongsController : ControllerBase
    {
        private readonly DbContextApp _db;
        public GetSongsController(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        [HttpPost]
        public ActionResult<IEnumerable<Song>> Index([FromForm] int userId)
        {
            return _db.Songs.Where(song => song.OwnerId.Equals(userId)).ToArray();
        }
    }
}