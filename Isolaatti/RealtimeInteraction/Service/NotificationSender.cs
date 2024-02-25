using System;
using System.Text;
using System.Text.Json;
using Isolaatti.DTOs;
using Isolaatti.Messaging;
using Isolaatti.Notifications.Entity;
using Isolaatti.RealtimeInteraction.Dto;
using RabbitMQ.Client;

namespace Isolaatti.RealtimeInteraction.Service;

public class NotificationSender
{
    private readonly IModel _channel;

    private const string Exchange = "default_exchange";
    private const string QueueName = "realtime_queue";
    private const string RoutingKey = "routing_realtime";
    
    public NotificationSender(Rabbitmq rabbitmq)
    {
        _channel = rabbitmq.GetConnection().CreateModel();
        _channel.ExchangeDeclare(Exchange, ExchangeType.Direct, true);
        _channel.QueueDeclare(QueueName, true, false, false);
        _channel.QueueBind(QueueName, Exchange, RoutingKey);
    }
    
    public void NotifyUser(int userId, NotificationEntity notificationEntity)
    {
        var dto = new RealtimeUnicastEventDto
        {
            UserId = userId,
            NotificationEntity = notificationEntity
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
    
    /// <summary>
    /// Sends an event that indicates that a comment was added to the discussion.
    /// </summary>
    /// <param name="comment"></param>
    /// <param name="clientId"></param>

    public void SendNewCommentEvent(CommentDto comment, Guid clientId)
    {

        var dto = new RealtimeMulticastEventDto<long>()
        {
            Type = EventType.CommentAdded,
            RelatedId = comment.Comment.PostId,
            Payload = comment,
            ClientId = clientId
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

    /// <summary>
    /// Sends an event that indicates that the post should be re rendered client side. This
    /// should be used when a post is modified or liked.
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="clientId"></param>
    public void SendPostUpdate(long postId, Guid clientId)
    {
        var dto = new RealtimeMulticastEventDto<long>()
        {
            Type = EventType.PostUpdate,
            Payload = null,
            ClientId = clientId,
            RelatedId = postId
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

    public void SendDeleteCommentEvent(long postId, long commentId, Guid clientId)
    {
        var dto = new RealtimeMulticastEventDto<long>()
        {
            Type = EventType.CommentRemoved,
            ClientId = clientId,
            RelatedId = postId,
            Payload = commentId
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

    public void SendCommentModifiedEvent(CommentDto updatedComment, Guid clientId)
    {
        var dto = new RealtimeMulticastEventDto<long>()
        {
            Type = EventType.CommentModified,
            ClientId = clientId,
            RelatedId = updatedComment.Comment.PostId,
            Payload = updatedComment
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