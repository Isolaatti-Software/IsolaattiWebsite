using System;

namespace isolaatti_API.Models
{
    public class SquadTopic
    {
        public Guid Id { get; set; }
        public Guid SquadId { get; set; }
        public Guid TopicId { get; set; }
    }
}