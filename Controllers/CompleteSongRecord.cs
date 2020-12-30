/*
* Isolaatti project
* Erik Cavazos, 2020
* This program is not allowed to be copied or reused without explicit permission.
* erik10cavazos@gmail.com and everardo.cavazoshrnnd@uanl.edu.mx
*/
using System;
using isolaatti_API.Classes;
using isolaatti_API.isolaatti_lib;
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
        public void Index(int songId, string bassUrl, string drumsUrl, string voiceUrl, string otherUrl, string uid)
        {
            try
            {
                Song recordToComplete = _dbContext.Songs.Find(songId);
                recordToComplete.BassUrl = bassUrl;
                recordToComplete.DrumsUrl = drumsUrl;
                recordToComplete.OtherUrl = otherUrl;
                recordToComplete.VoiceUrl = voiceUrl;
                recordToComplete.Uid = uid;
                recordToComplete.IsBeingProcessed = false;
                _dbContext.Songs.Update(recordToComplete);
                _dbContext.SaveChanges();

                int userId = _dbContext.Songs.Find(recordToComplete.Id).OwnerId;
                
                // add here some code to decide if should send an email
                
                User user = _dbContext.Users.Find(userId);
                if (user.NotifyByEmail)
                {
                    var emailNotificationSender = new EmailNotification(
                        _dbContext,
                        userId,
                        EmailNotification.EmailNotificationSongReady,
                        songId
                        );
                    emailNotificationSender.Send();
                }


                if (user.NotifyWhenProcessFinishes)
                {
                    // sends a notification to the user
                    NotificationSender notificationSender = new NotificationSender(
                        NotificationSender.NotificationModeProcessesFinished,
                        recordToComplete,
                        user,
                        _dbContext
                    );
                    notificationSender.Send();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}