using Isolaatti.Audios.Data;

namespace Isolaatti.Audios.Dto;

public class FeedAudio : Audio
{
    public FeedAudio(Audio audio)
    {
        Id = audio.Id;
        Name = audio.Name;
        CreationTime = audio.CreationTime;
        UserId = audio.UserId;
        FirestoreObjectPath = audio.FirestoreObjectPath;
    }

    public string UserName { get; set; }
}