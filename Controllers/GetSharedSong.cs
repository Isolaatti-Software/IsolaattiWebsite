using System.Collections.Generic;
using System.Linq;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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