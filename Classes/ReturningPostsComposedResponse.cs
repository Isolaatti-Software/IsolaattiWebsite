using isolaatti_API.Models;

namespace isolaatti_API.Classes
{
    public class ReturningPostsComposedResponse : SimpleTextPost
    {
        public ReturningPostsComposedResponse(SimpleTextPost post)
        {
            Id = post.Id;
            TextContent = post.TextContent;
            UserId = post.UserId;
            NumberOfLikes = post.NumberOfLikes;
            Privacy = post.Privacy;
            AudioAttachedUrl = post.AudioAttachedUrl;
            NumberOfComments = post.NumberOfComments;
        }
        public string UserName { get; set; }
        public bool Liked { get; set; }
    }
}