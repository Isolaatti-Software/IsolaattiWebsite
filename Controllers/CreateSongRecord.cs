/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class CreateSongRecord : ControllerBase
    {
        private readonly DbContextApp _contextApp;

        public CreateSongRecord(DbContextApp contextApp)
        {
            _contextApp = contextApp;
        }
        [HttpPost]
        public int Index([FromForm]int userId, [FromForm]string fileName, [FromForm]string songArtist="Unknown")
        {
            Song songToAdd = new Song()
            {
                OwnerId = userId,
                OriginalFileName = fileName,
                Artist = songArtist
            };
            _contextApp.Songs.Add(songToAdd);
            _contextApp.SaveChanges();
            
            return songToAdd.Id;
        }
    }
}