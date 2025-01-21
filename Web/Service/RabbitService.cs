using AuthService.Interface;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;


namespace AuthService.Service;

public class RabbitService : IRabbitService
{
    private IConfiguration _configuration;
    public RabbitService(IConfiguration configuration) 
    {
        _configuration = configuration;
    }

    public void SendMessage(Object obj) 
    {
        var message = JsonSerializer.Serialize(obj);
        SendMessage(message);
    }

    public void SendMessage(string message)
    {
        var factory = new ConnectionFactory() 
        {
            HostName = _configuration["RabbitMQ:Host"]
        };

        using (var connection = factory.CreateConnection()) 
        {
            using (var channel = connection.CreateModel()) 
            {
                channel.QueueDeclare(queue: _configuration["RabbitMQ:Queue"],
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                    routingKey: _configuration["RabbitMQ:Queue"],
                    basicProperties: null,
                    body: body);
            }
        }
    }

}
