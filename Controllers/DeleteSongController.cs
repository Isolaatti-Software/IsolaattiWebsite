/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DeleteSongController : ControllerBase
    {
        private readonly DbContextApp db;
        public DeleteSongController(DbContextApp contextApp)
        {
            db = contextApp;
        }
        // normal delete. includes: deleting song record from db, and deleting the files from server
        [HttpPost]
        public string Index([FromForm] int songId)
        {
            Song songToDelete = db.Songs.Find(songId);

            string uid = songToDelete.BassUrl.Split("/")[4];
            // deletes database record of song
            db.Songs.Remove(songToDelete);
            db.SaveChanges();

            return uid;
        }
    }
}