using System;

namespace isolaatti_API.Classes.ApiEndpointsRequestDataModels
{
    public class MakeCommentModel
    {
        public string Content { get; set; }
        public Guid? AudioId { get; set; }
        public int Privacy { get; set; }
    }
}