using System;

namespace isolaatti_API.Models
{
    public class SessionToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }

        public SessionToken()
        {
            Token = Guid.NewGuid().ToString();
        }
    }
}