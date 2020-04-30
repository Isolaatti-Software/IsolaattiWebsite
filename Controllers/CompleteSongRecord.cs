using System;
using isolaatti_API.Classes;
using isolaatti_API.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using MimeKit;

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

                int userId = _dbContext.Songs.Find(recordToComplete.Id).OwnerId;
                
                // add here some code to decide if should send an email
                
                sendEmailToUser(userId, songId);
                User user = _dbContext.Users.Find(userId);

                // sends a notification to the user
                NotificationSender notificationSender = new NotificationSender(
                    NotificationSender.NotificationModeProcessesFinished,
                        recordToComplete,
                        user,
                        _dbContext
                    );
                    notificationSender.Send();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }

        void sendEmailToUser(int userId, int songId)
        {
            string email = _dbContext.Users.Find(userId).Email;
            Song song = _dbContext.Songs.Find(songId);
            string songName = song.OriginalFileName;

            //urls
            string bass = song.BassUrl;
            string drums = song.DrumsUrl;
            string vocals = song.VoiceUrl;
            string others = song.OtherUrl;

            string htmlBody = String.Format(@"
<html>
    <body>
        <h1>Song {0} has been processed!</h1>
        <p>Your 4 tracks are now available on the app. Also, here you have direct links to the files:</p>
        <ul>
            <li>Bass: {1}</li>
            <li>Drums: {2}</li>
            <li>Vocals: {3}</li>
            <li>Other: {4}</li>
        </ul>
        <p>Be careful, these files will be delated 12 hours after they were created, so download them soon!</p>
    </body>
</html>",songName,bass,drums,vocals,others);

            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Isolaatti", "validation.isolaatti@gmail.com"));
            message.To.Add(new MailboxAddress(email));
            message.Subject = String.Format("Your song: {0} has been processed!",songName);
            message.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlBody
            };

            using (var client = new SmtpClient())
            {
                client.ServerCertificateValidationCallback = (s, c, h, e) => true;
                client.Connect("smtp.gmail.com", 465, true);
                client.Authenticate("validation.isolaatti@gmail.com", "0203_0302_");
                client.Send(message);
                client.Disconnect(true);
            }
        }
        
    }
}