using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace Isolaatti.Messaging;

public class Rabbitmq
{
    private readonly IConnection _connection;
    
    public Rabbitmq(IOptions<RabbitmqConfig> rabbitmqConfig)
    {
        var connectionFactory = new ConnectionFactory
        {
            UserName = rabbitmqConfig.Value.Username,
            Password = rabbitmqConfig.Value.Password,
            VirtualHost = rabbitmqConfig.Value.VirtualHost,
            HostName = rabbitmqConfig.Value.Host
        };

        _connection = connectionFactory.CreateConnection("isolaatti monolithic web api");
    }

    public IConnection GetConnection()
    {
        return _connection;
    }
}
