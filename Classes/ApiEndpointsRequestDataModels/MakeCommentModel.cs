using System;

namespace Isolaatti.Classes.ApiEndpointsRequestDataModels
{
    public class MakeCommentModel
    {
        public string Content { get; set; }
        public Guid? AudioId { get; set; }
        public int Privacy { get; set; }
    }
}