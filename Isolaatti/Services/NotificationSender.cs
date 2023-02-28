using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Isolaatti.Config;
using Isolaatti.DTOs;
using Isolaatti.Models.MongoDB;
using Isolaatti.Repositories;
using Microsoft.Extensions.Options;

namespace Isolaatti.Services;

public class NotificationSender
{
    private readonly SocketIoServiceKeysRepository _keysRepository;
    private readonly Servers _servers;

    public NotificationSender(IOptions<Servers> servers,SocketIoServiceKeysRepository keysRepository)
    {
        _keysRepository = keysRepository;
        _servers = servers.Value;
    }
    
    public async Task NotifyUser(int userId, SocialNotification notification)
    {
        var secret = await _keysRepository.CreateKey();
        var httpClient = new HttpClient();
        var content = JsonContent.Create(new
        {
            secret = secret.Key,
            userId = userId,
            data = notification
        });

        try
        {
            await httpClient.PostAsync($"{_servers.RealtimeServerUrl}/send_notification", content);
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
        var secret = await _keysRepository.CreateKey();
        var httpClient = new HttpClient();
        var content = JsonContent.Create(new
        {
            secret = secret.Key,
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
            await httpClient.PostAsync($"{_servers.RealtimeServerUrl}/event", content);
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
        var secret = await _keysRepository.CreateKey();
        var httpClient = new HttpClient();
        var content = JsonContent.Create(new
        {
            secret = secret.Key,
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
            await httpClient.PostAsync($"{_servers.RealtimeServerUrl}/event", content);
        }
        catch(HttpRequestException){ }
    }

    public async Task SendDeleteCommentEvent(long postId, long commentId, Guid clientId)
    {
        var secret = await _keysRepository.CreateKey();
        var httpClient = new HttpClient();
        var content = JsonContent.Create(new
        {
            secret = secret.Key,
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
            await httpClient.PostAsync($"{_servers.RealtimeServerUrl}/event", content);
        }
        catch(HttpRequestException){ }
    }

    public async Task SendCommentModifiedEvent(CommentDto updatedComment, Guid clientId)
    {
        var secret = await _keysRepository.CreateKey();
        var httpClient = new HttpClient();
        var content = JsonContent.Create(new
        {
            secret = secret.Key,
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
            await httpClient.PostAsync($"{_servers.RealtimeServerUrl}/event", content);
        }
        catch(HttpRequestException){ }
    }
}