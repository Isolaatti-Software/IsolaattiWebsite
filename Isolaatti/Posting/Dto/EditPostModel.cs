namespace Isolaatti.Posting.Dto
{
    public class EditPostModel : MakePostModel
    {
        public long PostId { get; set; }
        public bool RemoveAudio { get; set; } = false;
    }
}