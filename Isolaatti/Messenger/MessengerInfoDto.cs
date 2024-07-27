namespace Isolaatti.Messenger;

public class MessengerInfoDto
{
    public const string Success = "success";
    public const string AlreadyExists = "already_exists";
    public const string MessengerServerError = "messenger_server_error";
    
    public OpenfireAccount Account { get; set; }
    public string Domain { get; set; }
}