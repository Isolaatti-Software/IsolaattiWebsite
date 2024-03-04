namespace Isolaatti.Notifications.Entity;

public class NotificationPayloadEntity
{

    
    public string Type { get; set; }
    public int AuthorId { get; set; }
    public object Data { get; set; }
}