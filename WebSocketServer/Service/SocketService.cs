using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using RabbitMQ.Client.Events;
using RabbitMQ.Client;
using WebSocketServer.Interface;


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

    public List<string> Listen(IPAddress address) // слушает на постоянной основе есть ли подключения
    {
        //while (IsConnected)
        
            try
            {
            
                Socket server_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                server_socket.Bind(new IPEndPoint(IPAddress.Parse(_configuration["SocketSettings:Url"]), 15672));
                Console.WriteLine("Сокет слушает подключения");
                server_socket.Listen(10);
                var ipPoint = (IPEndPoint)server_socket.LocalEndPoint;
                if (ipPoint != null)
                {
                    var ip = ipPoint.Address.ToString();
                    var portIp = ipPoint.Port;
                    Console.WriteLine("Сокет начинает принимать подключения c адреса {0} на порту {1}", ip, portIp);
                }

                List<string> receivedMessage = _rabbitListener.ListenQueue(); // прослушивание очереди сообщений из рэбита

                server_socket.Dispose();
                Console.WriteLine("Сокет завершил прослушивание и закрыл соединение");

                return receivedMessage;
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Ошибка: {0}\nПричина: {1}\nМесто возникновения ошибки: {2}", ex.SocketErrorCode, ex.Message, ex.StackTrace);
            }
        
        Console.WriteLine(server_socket.Connected);
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

    public void Connect(string url, int port) 
    {
        try
        {
            Console.WriteLine("Попытка подключения к сервису...");
            server_socket.Connect(url, port);
            Console.WriteLine("Подключение прошло успешно: {0}", server_socket.Connected, server_socket.LocalEndPoint);
        }
        catch (SocketException ex) 
        {
            Console.WriteLine("Не удалось соединиться с сервисом: {0}", ex.Message);
        }
    } // соединяет с сервисом

    public void Close() 
    {
        Console.WriteLine("Идет закрытие соединения...");
        server_socket.Close();
        Console.WriteLine("Соединение открыто: {0}", server_socket.Connected);
    }// закрывает сокет
}
