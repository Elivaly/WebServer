using System.Text;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using WebSocketServer.Interface;
using WebSocketServer.Schemas;

namespace WebSocketServer.Service;

public class RabbitListenerService : IRabbitListenerService
{
    private IConnection _connection;
    private IModel _channel;
    private IConfiguration _configuration;
    private EventingBasicConsumer _consumer;
    public static List<Message> messages = new List<Message>();
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
            Password = _configuration["RabbitMQ:Password"]
        };

        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _consumer = new EventingBasicConsumer(_channel);

        _channel.QueueDeclare(
            queue: _configuration["RabbitMQ:Queue"],
            durable: false,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void ListenQueue()
    {
        _consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);
            var test = JObject.Parse(message);
            message = test["Message_Text"].ToString();

            Message mess = new Message();
            mess.Message_Text = message;
            messages.Add(mess);

            Console.WriteLine("[x] Получено сообщение {0}", message);
        };

        _channel.BasicQos(0, 1, false);

        _channel.BasicConsume(
            queue: _configuration["RabbitMQ:Queue"],
            autoAck: true,
            consumer: _consumer);
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
    }

}

