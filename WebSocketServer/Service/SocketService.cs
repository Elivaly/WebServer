using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Extensions.Hosting;
using WebSocketServer.Interface;


namespace WebSocketServer.Service;

public class SocketService: ISocketService
{
    private readonly IConfiguration _configuration;
    private readonly IRabbitListenerService _rabbitListener;
    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public SocketService(IConfiguration configuration, IRabbitListenerService rabbitListener)
    {
        _configuration = configuration;
        _rabbitListener = rabbitListener;
    }

    public void Listen(IPAddress address) // слушает на постоянной основе есть ли подключения
    {
        bool IsConnected = CheckSocketConnection(socket);
        Console.WriteLine(socket.RemoteEndPoint);
        Console.WriteLine(socket.LocalEndPoint);
        while (IsConnected)
        {
            try
            {
                Socket listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listen_socket.Bind(new IPEndPoint(IPAddress.Parse(_configuration["SocketSettings:Url"]), int.Parse(_configuration["SocketSettings:Port"])));
                Console.WriteLine("Сокет слушает подключения");
                listen_socket.Listen(10);
                var ipPoint = (IPEndPoint)listen_socket.LocalEndPoint;
                if (ipPoint != null)
                {
                    var ip = ipPoint.Address.ToString();
                    var portIp = ipPoint.Port;
                    Console.WriteLine("Сокет начинает принимать подключения c адреса {0} на порту {1}", ip, portIp);
                }


                //_rabbitListener.ListenQueue(null);

                Socket accept_socket = listen_socket.Accept();
                Console.WriteLine("Словилось подключение по адресу {0}", accept_socket.RemoteEndPoint);

                byte[] buffer = new byte[1024];
                int bytesReceived = accept_socket.Receive(buffer);
                string receivedMessage = Encoding.UTF8.GetString(buffer, 0, bytesReceived);
                Console.WriteLine("Получено сообщение: {0}", receivedMessage);


                accept_socket.Dispose();
                listen_socket.Dispose();
                Console.WriteLine("Сокет завершил прослушивание и закрыл соединение");
            }
            catch (SocketException ex)
            {
                Console.WriteLine("Ошибка: {0}\nПричина: {1}\nМесто возникновения ошибки: {2}", ex.SocketErrorCode, ex.Message, ex.StackTrace);
            }
        }
        Console.WriteLine(socket.Connected);
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
            socket.Connect(url, port);
            Console.WriteLine("Подключение прошло успешно: {0}", socket.Connected, socket.LocalEndPoint);
        }
        catch (SocketException ex) 
        {
            Console.WriteLine("Не удалось соединиться с сервисом: {0}", ex.Message);
        }
    } // соединяет с сервисом

    public void Close() 
    {
        Console.WriteLine("Идет закрытие соединения...");
        socket.Close();
        Console.WriteLine("Соединение открыто: {0}", socket.Connected);
    }// закрывает сокет
}
