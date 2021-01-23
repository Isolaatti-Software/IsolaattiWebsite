/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("/api/[controller]")]
    [ApiController]
    public class GetSongsController : ControllerBase
    {
        private readonly DbContextApp _db;
        public GetSongsController(DbContextApp dbContextApp)
        {
            _db = dbContextApp;
        }
        [HttpPost]
        public IQueryable<Song> Index([FromForm] int userId)
        {
            return _db.Songs
                .Where(song => song.OwnerId.Equals(userId) && !song.IsBeingProcessed);
        }

        [HttpPost]
        [Route("Processing")]
        public IQueryable<Song> Processing([FromForm] int userId)
        {
            return _db.Songs
                .Where(song => song.OwnerId.Equals(userId) && song.IsBeingProcessed);
        }
    }
}