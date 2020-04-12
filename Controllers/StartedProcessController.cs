using isolaatti_API.Classes;
using Microsoft.AspNetCore.Mvc;
using isolaatti_API.Models;

namespace isolaatti_API.Controllers
{
    [Route("[controller]")]
    public class StartedProcessController : ControllerBase
    {
        private readonly DbContextApp db;
        public StartedProcessController(DbContextApp dbContextApp)
        {
            db = dbContextApp;
        }
        [HttpPost]
        public void Index(int songId)
        {
            var song = db.Songs.Find(songId);
            song.IsBeingProcessed = true;
            db.Songs.Update(song);
            db.SaveChanges();
            User user = db.Users.Find(song.OwnerId);
            NotificationSender notificationSender = new NotificationSender(
                NotificationSender.NotificationModeSongStartedToProcess,
                user.GoogleToken,song.OriginalFileName,song.Artist
            );
            notificationSender.Send();
        }
    }
}