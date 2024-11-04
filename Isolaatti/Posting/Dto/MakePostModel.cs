using System;
using System.Collections.Generic;

namespace Isolaatti.Posting.Dto
{
    public class MakePostModel
    {
        public int Privacy { get; set; }
        public string Content { get; set; }
        public string? AudioId { get; set; }
        public Guid? SquadId { get; set; }
        public bool IsDraft { get; set; } = false;
        public List<Guid> Images { get; set; } = [];
    }
}