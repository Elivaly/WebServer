using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using WebSocketServer.Interface;
using WebSocketServer.Schems;


namespace WebSocketServer.Service;

public class SocketService: ISocketService
{
    private readonly IConfiguration _configuration;
    private readonly IRabbitListenerService _rabbitListener;
    Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public SocketService(IConfiguration configuration, IRabbitListenerService rabbitListener)
    {
        _configuration = configuration;
        _rabbitListener = rabbitListener;
    }

    public List<string> Listen(IPAddress address)
    {
        try
        {

            Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            server_socket.Bind(new IPEndPoint(IPAddress.Parse(_configuration["SocketSettings:Url"]), 15672));
            Console.WriteLine("Сокет слушает очередь");
            server_socket.Listen(10);

            List<string> receivedMessage = _rabbitListener.GetMessages();
            Console.WriteLine(receivedMessage.Count);
            foreach (var message in receivedMessage)
            {
                Console.WriteLine("Сообщение: {0}", message);
            }

            server_socket.Dispose();
            Console.WriteLine("Сокет завершил прослушивание и закрыл соединение");

            return receivedMessage;
        }
        catch (SocketException ex)
        {
            Console.WriteLine("Ошибка: {0}\nПричина: {1}\nМесто возникновения ошибки: {2}", ex.SocketErrorCode, ex.Message, ex.StackTrace);
        }
        return [];
    }

    public bool CheckSocketConnection(Socket socket)
    {
        if (socket == null)
        {
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(_configuration["SocketSettings:Url"]), int.Parse(_configuration["SocketSettings:Port"]));
            socket.Connect(endPoint);
        }
        return socket.Connected;
    }

    public void Close() 
    {
        Console.WriteLine("Идет закрытие соединения...");
        server_socket.Close();
        Console.WriteLine("Соединение открыто: {0}", server_socket.Connected);
    }
}
