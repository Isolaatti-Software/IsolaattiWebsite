using System;

namespace isolaatti_API.Models
{
    public class UserToken
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}