namespace Isolaatti.Notifications.Dto;

public class RegisterDeviceMessagingDto
{
    public int UserId { get; set; }
    public string SessionId { get; set; }
    public string Token { get; set; }
}