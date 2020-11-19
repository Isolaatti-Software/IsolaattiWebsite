using System;
using isolaatti_API.Models;
using MailKit.Net.Smtp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MimeKit;

namespace isolaatti_API.isolaatti_lib
{
    public class EmailNotification
    {
        public const int EmailNotificationSongReady = 0;
        public const int EmailNotificationSongProcessingStarted = 1;
        

        private readonly DbContextApp db;
        private int userId, songId, notificationType;
        
        /* Use this constructor to send notification about the state of the song being processed*/
        public EmailNotification(DbContextApp dbContext,int userId, int notificationType, int songId)
        {
            db = dbContext;
            this.userId = userId;
            this.songId = songId;
            this.notificationType = notificationType;
        }

        /* Use this constructor to send informative emails*/
        public EmailNotification(DbContextApp dbContext, int userId, string subject, string body)
        {
            db = dbContext;
            this.userId = userId;
            notificationType = -1;
        }

        public bool Send()
        {
            string email = db.Users.Find(userId).Email;
            Song song = db.Songs.Find(songId);
            
            // these are common
            string songName = song.OriginalFileName;
            string htmlBody;
            MimeMessage message = new MimeMessage();
            message.From.Add(new MailboxAddress("Isolaatti", "validation.isolaatti@gmail.com"));
            message.To.Add(new MailboxAddress(email));
            
            switch (notificationType)
            {
                case EmailNotificationSongReady:
                    //urls
                    string bass = song.BassUrl;
                    string drums = song.DrumsUrl;
                    string vocals = song.VoiceUrl;
                    string others = song.OtherUrl;
                    htmlBody = string.Format(EmailTemplates.SongReady,songName,bass,drums,vocals,others);
                    message.Subject = $"Your song: {songName} has been processed!";
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
                    } break;
                case EmailNotificationSongProcessingStarted: 
                    htmlBody = string.Format(EmailTemplates.SongStartedProcessing,songName);
                    message.Subject = $"Your song: {songName} is being processed!";
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
                    } break;
            }
            return true;
        }
    }
}