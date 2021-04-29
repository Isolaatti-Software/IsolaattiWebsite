using System;

namespace isolaatti_API.Models
{
    public class SessionToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Date { get; set; }

        public SessionToken()
        {
            Token = Guid.NewGuid().ToString();
            Date = DateTime.Now;
        }
    }
}