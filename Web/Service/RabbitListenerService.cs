﻿using System.Text;
using AuthService.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace AuthService.Service;

public class RabbitListenerService : IRabbitListenerService
{
    private readonly IConfiguration _configuration;
    private IConnection _connection;
    private IModel _channel;

    public RabbitListenerService(IConfiguration configuration) 
    {
        _configuration = configuration;
        Initialize();
    }

    public void Initialize() 
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:Host"],
            UserName = _configuration["RabbitMQ:User"],
            Password = _configuration["RabbitMQ:Password"],
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.QueueDeclare(
            queue: _configuration["RabbitMQ:Queue"],
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void ListenQueue(Object obj) 
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            Console.WriteLine("[x] Получено {0}",message);
        };

        _channel.BasicConsume(
            queue: _configuration["RabbitMQ:Queue"],
            autoAck: true,
            consumer: consumer);
    }

}
