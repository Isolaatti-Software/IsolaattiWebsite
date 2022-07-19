using System;

namespace Isolaatti.Classes.ApiEndpointsRequestDataModels
{
    public class MakePostModel
    {
        public int Privacy { get; set; }
        public string Content { get; set; }
        public PostTheme Theme { get; set; }
        public Guid? AudioId { get; set; }
    }
}