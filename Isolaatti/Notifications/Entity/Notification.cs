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

       

    }

    
}