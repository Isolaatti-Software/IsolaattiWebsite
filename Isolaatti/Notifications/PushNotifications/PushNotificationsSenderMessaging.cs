using System.Text;
using System.Text.Json;
using Isolaatti.Messaging;
using Isolaatti.Notifications.Dto;
using RabbitMQ.Client;

namespace Isolaatti.Notifications.PushNotifications;

public class PushNotificationsSenderMessaging
{
    private readonly IModel _channel;

    private const string Exchange = "default_exchange";
    private const string QueueName = "notification_send_queue";
    private const string RoutingKey = "routing_push_notifications";
    
    public PushNotificationsSenderMessaging(Rabbitmq rabbitmq)
    {
        _channel = rabbitmq.GetConnection().CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Direct, true);
        _channel.QueueDeclare(QueueName, true, false, false);
        _channel.QueueBind(QueueName, Exchange, RoutingKey);
    }

    public void SendSingleNotification(NotificationDto dto)
    {
        var props = _channel.CreateBasicProperties();

        props.ContentType = "application/json";
        props.DeliveryMode = 2;
        _channel.BasicPublish(Exchange, RoutingKey, props, Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto)));
    }
}