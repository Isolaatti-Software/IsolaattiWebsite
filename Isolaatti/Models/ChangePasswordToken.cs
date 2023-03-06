using System;
using System.Security.Cryptography;

namespace Isolaatti.Models
{
    public class ChangePasswordToken
    {
        public Guid Id { get; set; }
        public int UserId { get; set; }
        public string Token { get; set; }
        public DateTime Expires { get; set; }

        public ChangePasswordToken()
        {
            byte[] randomData = new byte[256];
            RandomNumberGenerator.Create().GetBytes(randomData);
            Token = Convert.ToBase64String(randomData);
            Expires = DateTime.Now.AddDays(1).ToUniversalTime();
        }
    }
}