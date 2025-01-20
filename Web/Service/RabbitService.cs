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

    public async void SendMessage(string message)
    {
        // Не забудьте вынести значения "localhost" и "MyQueue"
        // в файл конфигурации
        var factory = new ConnectionFactory() { HostName = _configuration["ApplicationHost:Address"] };
        using (var connection = await factory.CreateConnectionAsync())
        {
            using (var channel = await connection.CreateModel())
            {
                channel.QueueDeclare(queue: "MyQueue",
                               durable: false,
                               exclusive: false,
                               autoDelete: false,
                               arguments: null);

                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                               routingKey: "MyQueue",
                               basicProperties: null,
                               body: body);
            }
        }
    }

}
