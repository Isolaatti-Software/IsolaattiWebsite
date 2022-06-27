using System;

namespace isolaatti_API.Models
{
    public class ExternalUser
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string GoogleUid { get; set; }
    }
}