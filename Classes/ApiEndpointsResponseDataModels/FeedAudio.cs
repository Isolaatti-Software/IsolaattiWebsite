using isolaatti_API.Models;

namespace isolaatti_API.Classes.ApiEndpointsResponseDataModels;

public class FeedAudio : Audio
{
    public FeedAudio(Audio audio)
    {
        Id = audio.Id;
        Name = audio.Name;
        CreatedAt = audio.CreatedAt;
        UserId = audio.UserId;
    }

    public string UserName { get; set; }
}