using System.Collections.Generic;
using FirebaseAdmin.Messaging;
using isolaatti_API.Models;

namespace isolaatti_API.Classes
{
    public class NotificationSender
    {
        /* These constants are passed to the constructor */
        public const int NotificationModeProcessesFinished = 1;
        public const int NotificationModeSongStartedToProcess = 2;

        private int type;
        private string songName;
        private string songArtist;
        private string userToken;

        /*
         * Use this constructor when you want to tell the user that a song has been processed, or
         * when a song has started to be processed
         */
        public NotificationSender(int type,string userToken, string songName, string songArtist)
        {
            this.type = type;
            this.userToken = userToken;
            this.songName = songName;
            this.songArtist = songArtist;
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
                        {"songName",songName},
                        {"songArtist",songArtist}
                    }; break;
                case NotificationModeSongStartedToProcess: 
                    message.Data = new Dictionary<string, string>()
                    {
                        {"type",NotificationModeSongStartedToProcess.ToString()},
                        {"songName",songName},
                        {"songArtist",songArtist}
                    }; break;
            }
            message.Token = registrationToken;
            string response = await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }
    }
}