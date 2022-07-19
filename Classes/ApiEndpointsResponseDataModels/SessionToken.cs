using System;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels
{
    public class SessionToken
    {
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string Token { get; set; }
    }
}