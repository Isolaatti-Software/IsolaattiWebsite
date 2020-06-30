using System;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace isolaatti_API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CreateSongRecord : ControllerBase
    {
        private readonly DbContextApp _contextApp;

        public CreateSongRecord(DbContextApp contextApp)
        {
            _contextApp = contextApp;
        }
        [HttpPost]
        public void Index([FromForm]int userId, [FromForm]string sourceUrl, [FromForm]string fileName, [FromForm]string songArtist="Unknown")
        {
            Song songToAdd = new Song()
            {
                OwnerId = userId,
                OriginalFileName = fileName,
                Artist = songArtist
            };
            _contextApp.Songs.Add(songToAdd);
            
            // add song to jobs queue
            SongQueue songQueue = new SongQueue()
            {
                AudioSourceUrl = sourceUrl,
                ReservationTime = DateTime.Now,
                SongId = songToAdd.Id,
                Reserved = false
            };
            _contextApp.SongsQueue.Add(songQueue);
            
            _contextApp.SaveChanges();
            
            
        }
    }
}