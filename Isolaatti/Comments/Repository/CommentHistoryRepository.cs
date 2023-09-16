using Isolaatti.Comments.Entity;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using MongoDB.Driver;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Isolaatti.Comments.Repository
{
    public class CommentHistoryRepository
    {
        private readonly IMongoCollection<CommentModificationHistory> _commentModificationCollection;
        public CommentHistoryRepository(MongoDatabase mongoDatabase) 
        {
            _commentModificationCollection = mongoDatabase.GetCommentModificationHistoryCollection();
        }

        public async Task InsertModificationHistory(Comment comment)
        {
            var id = comment.Id;

            await _commentModificationCollection.InsertOneAsync(new CommentModificationHistory()
            {
                CommentId = id,
                Comment = comment
            });

        }

        public async Task<List<CommentModificationHistory>> GetModificationHistory(long commentId)
        {
            var filter = Builders<CommentModificationHistory>.Filter.Eq(c => c.CommentId, commentId);
            return await _commentModificationCollection.Find<CommentModificationHistory>(filter).ToListAsync();
        }

    }
}
