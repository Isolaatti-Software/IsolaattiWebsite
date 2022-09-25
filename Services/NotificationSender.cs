using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Isolaatti.Classes.ApiEndpointsResponseDataModels;
using Isolaatti.Config;
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

    public async Task SendUpdateEvent(FeedComment comment)
    {
        var secret = await _keysRepository.CreateKey();
        var httpClient = new HttpClient();
        var content = JsonContent.Create(new
        {
            secret = secret.Key,
            eventData = new
            {
                type = "post",
                id = comment.PostId,
                data = comment
            }
        });

        try
        {
            await httpClient.PostAsync($"{_servers.RealtimeServerUrl}/update_event", content);
        }
        catch(HttpRequestException){ }
    }
}