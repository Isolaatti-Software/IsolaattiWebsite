using System;

namespace Isolaatti.Posting.Dto
{
    public class MakePostModel
    {
        public int Privacy { get; set; }
        public string Content { get; set; }
        public string? AudioId { get; set; }
        public Guid? SquadId { get; set; }
    }
}