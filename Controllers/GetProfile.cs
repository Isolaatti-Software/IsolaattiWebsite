using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class GetProfile : ControllerBase
    {
        private readonly DbContextApp Db;

        public GetProfile(DbContextApp _dbContext)
        {
            Db = _dbContext;
        }
        [HttpPost]
        public Profile Index([FromForm]int userId, [FromForm]string password)
        {
            var user = Db.Users.Find(userId);
            if (user.Password.Equals(password))
            {
                var profile = new Profile()
                {
                    Username = user.Name,
                    Email = user.Email,
                    NumberOfSongs = Db.Songs.Count(song => song.OwnerId.Equals(userId)),
                    NumberOfLinks = Db.SharedSongs.Count(sharedLink => sharedLink.userId.Equals(userId))
                };
                return profile;
            }

            return null;
        }
    }
}