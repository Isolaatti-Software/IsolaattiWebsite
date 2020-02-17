using System;
using isolaatti_API.Models;
using Microsoft.AspNetCore.Mvc;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class CompleteSongRecord : Controller
    {
        private readonly DbContextApp _dbContext;

        public CompleteSongRecord(DbContextApp contextApp)
        {
            _dbContext = contextApp;
        }
        [HttpPost]
        public void Index(int songId, string bassUrl, string drumsUrl, string voiceUrl, string otherUrl)
        {
            try
            {
                Song recordToComplete = _dbContext.Songs.Find(songId);
                recordToComplete.BassUrl = bassUrl;
                recordToComplete.DrumsUrl = drumsUrl;
                recordToComplete.OtherUrl = otherUrl;
                recordToComplete.VoiceUrl = voiceUrl;
                _dbContext.Songs.Update(recordToComplete);
                _dbContext.SaveChanges();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        
    }
}