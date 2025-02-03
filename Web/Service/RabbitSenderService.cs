using System.Text;
using System.Text.Json;
using AuthService.Interface;
using RabbitMQ.Client;


namespace AuthService.Service;

public class RabbitSenderService : IRabbitSenderService
{
    private IConfiguration _configuration;
    private IModel _channel;
    private IConnection _connection;
    public RabbitSenderService(IConfiguration configuration)
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
    public void SendMessage(Object obj)
    {
        var message = JsonSerializer.Serialize(obj);
        SendMessage(message);
    }

    public void SendMessage(string message)
    {
        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(
            exchange: "",
            routingKey: _configuration["RabbitMQ:Queue"],
            basicProperties: null,
            body: body);

        _channel.Close();
        _connection.Close();

    }

}
