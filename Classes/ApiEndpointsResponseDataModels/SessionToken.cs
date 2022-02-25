using System;

namespace isolaatti_API.Classes.ApiEndpointsResponseDataModels
{
    public class SessionToken
    {
        public DateTime Expires { get; set; }
        public DateTime Created { get; set; }
        public string Token { get; set; }
    }
}