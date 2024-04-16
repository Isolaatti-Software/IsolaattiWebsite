using System;
using System.Text.Json;
using Isolaatti.Accounts.SignIn.Dto;
using Isolaatti.Utils;

namespace Isolaatti.Accounts.SignIn.Data
{
    public class Session
    {
        public Guid Id { get; set; }
        
        public int UserId { get; set; }
        
        public string IpAddress { get; set; }
        
        public string UserAgent { get; set; }

        public string SessionKey { get; set; }
        
        public DateTime CreationDate { get; set; }

        public Session()
        {
            SessionKey = RandomData.GenerateRandomKey(32);
            CreationDate = DateTime.Now.ToUniversalTime();
        }

        public SessionDto GetDto() 
        {
            return new() 
            {
                SessionId = Id,
                SessionKey = SessionKey
            };
        }

        public override string ToString()
        {
            return JsonSerializer.Serialize<SessionDto>(GetDto());
        }
    }
}