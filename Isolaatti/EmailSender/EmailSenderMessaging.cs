using System.Text;
using System.Text.Json;
using Isolaatti.Messaging;
using RabbitMQ.Client;

namespace Isolaatti.EmailSender;

public class EmailSenderMessaging
{
    private readonly IModel _channel;

    private const string Exchange = "default_exchange";
    private const string QueueName = "email_send_queue";
    private const string RoutingKey = "routing_email";

    public EmailSenderMessaging(Rabbitmq rabbitmq)
    {
        _channel = rabbitmq.GetConnection().CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Direct, true);
        _channel.QueueDeclare(QueueName, true, false, false);
        _channel.QueueBind(QueueName, Exchange, RoutingKey);
    }

    public void SendEmail(string fromAddress, string fromName, string toAddress, string toName, string subject,
        string htmlBody)
    {
        var dto = new EmailDto()
        {
            FromAddress = fromAddress,
            FromName = fromName,
            ToAddress = toAddress,
            ToName = toName,
            Subject = subject,
            HtmlBody = htmlBody
        };
        var props = _channel.CreateBasicProperties();

        props.ContentType = "application/json";
        props.DeliveryMode = 2;
        _channel.BasicPublish(Exchange, RoutingKey, props, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto,
            new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })));
    }
}