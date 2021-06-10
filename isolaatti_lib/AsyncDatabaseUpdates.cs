using System.Linq;
using System.Threading.Tasks;
using isolaatti_API.Models;

namespace isolaatti_API.isolaatti_lib
{
    public static class AsyncDatabaseUpdates
    {
        public static async Task UpdateNumberOfComments(DbContextApp _dbContext, long postId)
        {
            var numberOfComments = _dbContext.Comments.Count(comment => comment.SimpleTextPostId.Equals(postId));
            var postToUpdate = _dbContext.SimpleTextPosts.Find(postId);
            postToUpdate.NumberOfComments = numberOfComments;
            _dbContext.SimpleTextPosts.Update(postToUpdate);
           await _dbContext.SaveChangesAsync();
        }
    }
}