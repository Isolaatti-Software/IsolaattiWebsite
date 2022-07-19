using System;
using System.Security.Cryptography;

namespace Isolaatti.Models
{
    public class SessionToken
    {
        public Guid Id { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public DateTime Date { get; set; }

        public SessionToken()
        {
            byte[] randomData = new byte[256];
            RandomNumberGenerator.Create().GetBytes(randomData);
            Token = Convert.ToBase64String(randomData);
            Date = DateTime.Now.ToUniversalTime();
        }
    }
}