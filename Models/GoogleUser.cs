using System;

namespace isolaatti_API.Models
{
    public class GoogleUser
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string GoogleUid { get; set; }
    }
}