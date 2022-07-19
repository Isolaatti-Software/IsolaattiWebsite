using Isolaatti.Models.AudiosMongoDB;

namespace Isolaatti.Classes.ApiEndpointsResponseDataModels;

public class FeedAudio : Audio
{
    public FeedAudio(Audio audio)
    {
        Id = audio.Id;
        Name = audio.Name;
        CreationTime = audio.CreationTime;
        UserId = audio.UserId;
    }

    public string UserName { get; set; }
}