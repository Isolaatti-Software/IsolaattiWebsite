namespace Isolaatti.EmailSender;

public class EmailDto
{
    public string FromAddress { get; set; }
    public string FromName { get; set; }
    public string ToAddress { get; set; }
    public string ToName { get; set; }
    public string Subject { get; set; }
    public string HtmlBody { get; set; }
}