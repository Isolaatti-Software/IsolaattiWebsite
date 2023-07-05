using System;
using System.ComponentModel.DataAnnotations.Schema;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Isolaatti.Notifications.Entity
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public int UserId { get; set; }
        public object Payload { get; set; }
        public bool Read { get; set; }

        public Notification()
        {
            TimeStamp = DateTime.Now.ToUniversalTime();
            Read = false;
        }

        public bool ReSend()
        {
            return DateTime.Now.ToUniversalTime() - TimeStamp > TimeSpan.FromMinutes(10);
        }

        public bool ShouldReinsert(Notification notification)
        {
            var payload = notification.Payload;

            if(payload == null)
            {
                return false;
            }

            return payload switch
            {
                LikeNotificationPayload likeNotificationPayload => 
                    likeNotificationPayload.PostId == (payload as LikeNotificationPayload).PostId && 
                    likeNotificationPayload.MakerUserId == (payload as LikeNotificationPayload).MakerUserId,
                FollowerNotificationPayload followerNotificationPayload =>
                    followerNotificationPayload.NewFollowerUserId == (payload as FollowerNotificationPayload).NewFollowerUserId,
                _ => false
            } && notification.UserId == UserId;
        }

       

    }

    
}