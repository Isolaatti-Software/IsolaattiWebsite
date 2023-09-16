using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Isolaatti.Comments.Entity
{
    public class CommentModificationHistory
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string HistoryId { get; set; }

        [Required]
        public long CommentId { get; set; }

        [Required]
        public Comment Comment { get; set; }
    }
}
