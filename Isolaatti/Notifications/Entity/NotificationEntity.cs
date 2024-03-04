using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;


namespace Isolaatti.Notifications.Entity
{
    public class NotificationEntity : IDisposable
    {
        
        public const string TypeLike = "like";
        public const string TypePostConversation = "comments";
        public const string TypeFollower = "follower";
        
        // keys
        public const string KeyType = "type";
        public const string KeyAuthorId = "authorId";
        public const string KeyAuthorName = "authorName";
        public const string KeyPostId = "postId";
        public const string KeyLikeId = "likeId";
        public const string KeyFollowerUserId = "followerUserId";
        
        public long Id { get; set; }
        public DateTime TimeStamp { get; set; }
        public int UserId { get; set; }
        public bool Read { get; set; }
        public long[] RelatedNotifications { get; set; }
        public JsonDocument Data { get; set; }

        public NotificationEntity()
        {
            TimeStamp = DateTime.Now.ToUniversalTime();
            Read = false;
        }

        public bool ReSend()
        {
            return DateTime.Now.ToUniversalTime() - TimeStamp > TimeSpan.FromMinutes(10);
        }

        public bool ShouldReinsert(NotificationEntity notificationEntity)
        {
            return true;
        }


        public void Dispose()
        {
            Data.Dispose();
        }
    }

    
}