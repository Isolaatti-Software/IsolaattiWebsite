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
    public class UnshareSong : Controller
    {
        private readonly DbContextApp db;

        public UnshareSong(DbContextApp _db)
        {
            db = _db;
        }
        [HttpPost]
        public void Index([FromForm] int userId, [FromForm] string pwd, [FromForm] int songId)
        {
            db.SharedSongs.Remove(
                db.SharedSongs.Single(s => s.SharedSongId.Equals(songId))
                );
            db.SaveChanges();
        }
    }
}