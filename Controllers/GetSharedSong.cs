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
    [Route("[controller]")]
    
    public class GetSharedSong : ControllerBase
    {
        private readonly DbContextApp db;

        public GetSharedSong(DbContextApp _dbContext)
        {
            db = _dbContext;
        }
        [HttpPost]
        public SharedSong Index([FromForm] string songUid)
        {
            var songRef = db.SharedSongs.Single(s => s.uid.Equals(songUid));
            var realSong = db.Songs.Find(songRef.SharedSongId);
            
            return new SharedSong()
            {
                OwnerId = realSong.OwnerId,
                Name = realSong.OriginalFileName,
                Artist = realSong.Artist,
                BassUrl = realSong.BassUrl,
                DrumsUrl = realSong.DrumsUrl,
                VoiceUrl = realSong.VoiceUrl,
                OtherUrl = realSong.OtherUrl
            };
        }
    }
}