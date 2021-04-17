using System;

namespace isolaatti_API.Models
{
    public class SessionToken
    {
        public int Id { get; set; }
        public string Token = Guid.NewGuid().ToString();
        public int UserId { get; set; }
    }
}