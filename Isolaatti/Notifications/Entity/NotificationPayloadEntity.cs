namespace Isolaatti.Notifications.Entity;

public class NotificationPayloadEntity
{
    public const string TypeLike = "like";
    public const string TypePostConversation = "comments";
    public const string TypeFollower = "follower";
    
    public string Type { get; set; }
    public int AuthorId { get; set; }
    
    // Information depending on the notification type. It can be a userId, squadId, a url, etc.
    // For TypeLike and TypePostConversation, this must be the postId to open
    // For TypeFollower this must be the userId of the new follower
    public string IntentData { get; set; }
}