using System;

namespace Isolaatti.Accounts.Data
{
    public class SessionToken
    {
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string Token { get; set; }
        public int UserId { get; set; }
    }
}