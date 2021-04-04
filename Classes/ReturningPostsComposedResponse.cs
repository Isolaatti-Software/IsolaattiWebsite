namespace isolaatti_API.Classes
{
    public class ReturningPostsComposedResponse
    {
        public long Id { get; set; }
        public string TextContent { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public long NumberOfLikes { get; set; }
        public int Privacy { get; set; }
        public bool Liked { get; set; }
    }
}