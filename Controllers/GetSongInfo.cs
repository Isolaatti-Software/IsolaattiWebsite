using System.Collections.Generic;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class GetSongInfo : ControllerBase
    {
        private readonly DbContextApp db;

        public GetSongInfo(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public Song Index([FromForm] int id)
        {
            return db.Songs.Find(id);
        }
    }
}