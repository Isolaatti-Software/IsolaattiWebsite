using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Isolaatti.Config;
using Isolaatti.DTOs;
using Isolaatti.Notifications.Entity;
using Isolaatti.RealtimeInteraction.Dto;
using Isolaatti.Services;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Isolaatti.RealtimeInteraction.Service;

public class NotificationSender
{
    private readonly Servers _servers;
    private readonly ILogger<NotificationSender> _logger;
    private readonly HttpClientSingleton _httpClientSingleton;
    private readonly IOptions<IsolaattiServicesKeys> _keys;

    public NotificationSender(IOptions<Servers> servers,IOptions<IsolaattiServicesKeys> serviceKeys, ILogger<NotificationSender> logger, HttpClientSingleton httpClientSingleton)
    {
        _servers = servers.Value;
        _logger = logger;
        _httpClientSingleton = httpClientSingleton;
        _keys = serviceKeys;
    }
    
    public async Task NotifyUser(int userId, Notification notification)
    {
        var content = JsonContent.Create(new
        {
            secret = _keys.Value.RealtimeService,
            userId,
            data = notification
        });

        try
        {
            await _httpClientSingleton.Client.PostAsync($"{_servers.RealtimeServerInternalUrl}/send_notification", content);
        }
        catch (HttpRequestException) { }
    }
    
    /// <summary>
    /// Sends an event that indicates that a comment was added to the discussion.
    /// </summary>
    /// <param name="comment"></param>
    /// <param name="clientId"></param>

    public async Task SendNewCommentEvent(CommentDto comment, Guid clientId)
    {
        
        var content = JsonContent.Create(new
        {
            secret = _keys.Value.RealtimeService,
            eventData = new RealtimeEventDto<long>()
            {
                Type = EventType.CommentAdded,
                RelatedId = comment.Comment.PostId,
                Payload = comment,
                ClientId = clientId
            }
        });
    
        try
        {
            await _httpClientSingleton.Client.PostAsync($"{_servers.RealtimeServerInternalUrl}/event", content);
        }
        catch(HttpRequestException){ }
    }

    /// <summary>
    /// Sends an event that indicates that the post should be re rendered client side. This
    /// should be used when a post is modified or liked.
    /// </summary>
    /// <param name="postId"></param>
    /// <param name="clientId"></param>
    public async Task SendPostUpdate(long postId, Guid clientId)
    {
        var content = JsonContent.Create(new
        {
            secret = _keys.Value.RealtimeService,
            eventData = new RealtimeEventDto<long>()
            {
                Type = EventType.PostUpdate,
                Payload = null,
                ClientId = clientId,
                RelatedId = postId
            }
        });

        try
        {
            await _httpClientSingleton.Client.PostAsync($"{_servers.RealtimeServerInternalUrl}/event", content);
        }
        catch (HttpRequestException e)
        {
            _logger.Log(LogLevel.Error,"Error making request");
        }
    }

    public async Task SendDeleteCommentEvent(long postId, long commentId, Guid clientId)
    {
        var content = JsonContent.Create(new
        {
            secret = _keys.Value.RealtimeService,
            eventData = new RealtimeEventDto<long>()
            {
                Type = EventType.CommentRemoved,
                ClientId = clientId,
                RelatedId = postId,
                Payload = commentId
            }
        });
    
        try
        {
            await _httpClientSingleton.Client.PostAsync($"{_servers.RealtimeServerInternalUrl}/event", content);
        }
        catch(HttpRequestException){ }
    }

    public async Task SendCommentModifiedEvent(CommentDto updatedComment, Guid clientId)
    {
        var content = JsonContent.Create(new
        {
            secret = _keys.Value.RealtimeService,
            eventData = new RealtimeEventDto<long>()
            {
                Type = EventType.CommentModified,
                ClientId = clientId,
                RelatedId = updatedComment.Comment.PostId,
                Payload = updatedComment
            }
        });
    
        try
        {
            await _httpClientSingleton.Client.PostAsync($"{_servers.RealtimeServerInternalUrl}/event", content);
        }
        catch(HttpRequestException){ }
    }
}