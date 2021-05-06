using System;

namespace isolaatti_API.Models
{
    public class AdminAccountSessionToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public int AccountId { get; set; }

        public AdminAccountSessionToken()
        {
            Token = Guid.NewGuid().ToString();
        }
    }
}