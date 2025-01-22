using Microsoft.AspNetCore.Connections;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace WebSocketServer.Service;

public class RabbitListenerService : BackgroundService
{
    private IConnection _connection;
    private IModel _channel;
    IConfiguration _configuration;
    public RabbitListenerService(IConfiguration configuration)
    {
        _configuration = configuration;
        var factory = new ConnectionFactory { HostName = _configuration["RabbitMQ:Host"] };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(queue: _configuration["RabbitMQ:Queue"],
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        stoppingToken.ThrowIfCancellationRequested();

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (ch, ea) =>
        {
            var content = Encoding.UTF8.GetString(ea.Body.ToArray());

            Debug.WriteLine($"Получено сообщение: {content}");
            Console.WriteLine(content);

            _channel.BasicAck(ea.DeliveryTag, false);
        };

        _channel.BasicConsume(_configuration["RabbitMQ:Queue"], false, consumer);

        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _channel.Close();
        _connection.Close();
        base.Dispose();
    }
}
