using System;

namespace Isolaatti.Classes.ApiEndpointsRequestDataModels
{
    public class MakePostModel
    {
        public int Privacy { get; set; }
        public string Content { get; set; }
        public string AudioId { get; set; }
        public Guid? SquadId { get; set; }
    }
}