using System.Collections.Generic;
using FirebaseAdmin.Messaging;
using isolaatti_API.Models;
using Notification = isolaatti_API.Models.Notification;

namespace isolaatti_API.Classes
{
    public class NotificationSender
    {
        /* These constants are passed to the constructor */
        public const int NotificationModeProcessesFinished = 1;
        public const int NotificationModeSongStartedToProcess = 2;

        private int type;
        private string userToken;
        private Song _song;
        private User _user;
        private readonly DbContextApp _db;

        /*
         * Use this constructor when you want to tell the user that a song has been processed, or
         * when a song has started to be processed
         */
        public NotificationSender(int type, Song song, User user,DbContextApp dbContext)
        {


            this.type = type;
            userToken = user.GoogleToken;
            _song = song;
            _user = user;
            _db = dbContext;
        }

        private void SaveNotification()
        {
            Notification notification = new Notification()
            {
                SongId = _song.Id,
                ArtistName = _song.Artist,
                Seen = false,
                SongName = _song.OriginalFileName,
                UserId = _user.Id,
                type = type
            };
            _db.Notifications.Add(notification);
            _db.SaveChanges();
        }

        public async void Send()
        {
            var registrationToken = userToken;
            var message = new Message();
            // creates the message, depending on the type
            switch (type)
            {
                case NotificationModeProcessesFinished:
                    message.Data = new Dictionary<string, string>()
                    {
                        {"type",NotificationModeProcessesFinished.ToString()},
                        {"songName",_song.OriginalFileName},
                        {"songArtist",_song.Artist}
                    }; break;
                case NotificationModeSongStartedToProcess: 
                    message.Data = new Dictionary<string, string>()
                    {
                        {"type",NotificationModeSongStartedToProcess.ToString()},
                        {"songName",_song.OriginalFileName},
                        {"songArtist",_song.Artist}
                    }; break;
            }
            SaveNotification();
            message.Token = registrationToken;
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}