using System;
using System.Text;
using System.Text.Json;
using Isolaatti.Accounts.SignIn.Dto;
using Isolaatti.Utils;

namespace Isolaatti.Accounts.SignIn.Data
{
    public class Session
    {
        public string Id { get; set; }
        
        public int UserId { get; set; }
        
        public string IpAddress { get; set; }
        
        public string UserAgent { get; set; }
        
        
        public DateTime CreationDate { get; set; }

        public Session()
        {
            Id = RandomData.GenerateRandomKey(64);
            CreationDate = DateTime.Now.ToUniversalTime();
        }
        
        public override string ToString()
        {
            return Id;
        }
    }
}