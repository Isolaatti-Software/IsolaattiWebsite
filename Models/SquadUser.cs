using System;

namespace isolaatti_API.Models
{
    public class SquadUser
    {
        public Guid Id { get; set; }
        public Guid SquadId { get; set; }
        public int UserId { get; set; }
    }
}