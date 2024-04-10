using System.ComponentModel.DataAnnotations;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

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
