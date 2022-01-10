namespace isolaatti_API.Classes.ApiEndpointsRequestDataModels
{
    public class Border
    {
        public string Color { get; set; }
        public string Type { get; set; }
        public int Size { get; set; }
        public int Radius { get; set; }
    }

    public class Background
    {
        public string Type { get; set; }
        public string[] Colors { get; set; }
        public int Direction { get; set; }
    }

    public class PostTheme
    {
        public bool Existing { get; set; }
        public string FontColor { get; set; }
        public string BackgroundColor { get; set; }
        public bool Gradient { get; set; }
        public Border Border { get; set; }
        public Background Background { get; set; }
    }
}