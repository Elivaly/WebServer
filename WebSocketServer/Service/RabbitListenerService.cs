﻿using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Connections;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks.Dataflow;
using WebSocketServer.Interface;
using WebSocketServer.Schems;

namespace WebSocketServer.Service;

public class RabbitListenerService : BackgroundService, IRabbitListenerService
{
    private IConnection _connection;
    private IModel _channel;
    private IConfiguration _configuration;
    public static List<Message> messages = new List<Message>();
    public RabbitListenerService(IConfiguration configuration)
    {
        _configuration = configuration;
        Initialize();
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

    public void Initialize()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _configuration["RabbitMQ:Host"],
            UserName = _configuration["RabbitMQ:User"],
            Password = _configuration["RabbitMQ:Password"]
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

    public void ListenQueue()
    {
        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            var test = JObject.Parse(message);
            message = test["Message_Text"].ToString();
            Message mess = new Message();
            mess.Message_Text = message;
            messages.Add(mess);

            Console.WriteLine("[x] Получено {0}", message);
        };

        _channel.BasicQos(0,1,false);

        _channel.BasicConsume(
            queue: _configuration["RabbitMQ:Queue"],
            autoAck: true,
            consumer: consumer);

        _connection.Close();
        _channel.Close();
    }
    public List<string> GetMessages() 
    {
        List<string> messagesString = new List<string>();
        foreach (var message in messages) 
        {
            string text = message.Message_Text;  
            messagesString.Add(text);
        }
        return messagesString;
    }

    public void ClearList() 
    {
        messages.Clear();
    }

    public void CloseConnection()
    {
        _channel.Close();
        _connection.Close();
        _channel.Dispose();
        _connection.Dispose();
        base.Dispose();
    }

}

