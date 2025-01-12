using Isolaatti.Audios.Data;

namespace Isolaatti.Audios.Dto;

public class FeedAudio : Audio
{
    public FeedAudio(Audio audio)
    {
        Id = audio.Id;
        CreationTime = audio.CreationTime;
        UserId = audio.UserId;
        FirestoreObjectPath = audio.FirestoreObjectPath;
        DurationSeconds = audio.DurationSeconds;
    }

    public string? UserName { get; set; }
}