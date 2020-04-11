using FirebaseAdmin.Messaging;
using isolaatti_API.Models;

namespace isolaatti_API.Classes
{
    public class NotificationSender
    {
        public static int NotificationModeProcessesFinished = 1;

        public static int NotificationModeSongStartedToProcess = 2;

        private int type;
        private int songId;
        private int userId;

        /*
         * Use this constructor when you want to tell the user that a song has been processed, or
         * when a song has started to be processed
         */
        public NotificationSender(int type,int userId,int songId)
        {
            this.type = type;
            this.userId = userId;
            this.songId = songId;
        }

        public void Send()
        {
            /* Write here the code to send a message using the google cloud sdk */
        }
    }
}