namespace isolaatti_API.Classes.ApiEndpointsRequestDataModels
{
    public class MakePostModel
    {
        public int Privacy { get; set; }
        public string Content { get; set; }
        public PostTheme Theme { get; set; }
        public string AudioUrl { get; set; }
    }
}