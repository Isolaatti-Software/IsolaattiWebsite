/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
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