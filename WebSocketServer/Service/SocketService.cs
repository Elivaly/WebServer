using System.IO;
using System.Net;
using System.Net.Sockets;
using WebSocketServer.Interface;


namespace WebSocketServer.Service;

public class SocketService: ISocketService
{
    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private readonly IConfiguration _configuration;
    private bool IsConnected;
    private int port;
    private string url;
    public SocketService(IConfiguration configuration)
    {
        _configuration = configuration;
        url = _configuration["SocketSettings:Url"];
        port = int.Parse(_configuration["SocketSettings:Port"]);
    }

    public void Accept() // получает входящее подключенние
    { 
    
    }

    public IPEndPoint CreateEndPoint() // создает локальку
    {
        IPAddress ip = IPAddress.Parse(url);
        IPEndPoint ep = new IPEndPoint(ip, port);
        return ep;
    }
    public void Bind(IPEndPoint endPoint) // привязывает к локальной точке
    {
        try
        {
            socket.Bind(endPoint);
            Console.WriteLine("Удалось привязать локальную точку: {0}",socket.LocalEndPoint);
        }
        catch (SocketException ex) 
        {
            Console.WriteLine("Не удалось привязать локальную точку: {0}\nКод ошибки: {1},{2}\nТрек: {3}", ex.Message, ex.ErrorCode, ex.SocketErrorCode, ex.StackTrace);
        }
    }

    public void Listen(IPAddress address, int port) // слушает на постоянной основе есть ли подключения
    {
        if(!IsConnected) 
        {
            if(socket == null) 
            {
                socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            socket.Listen(10);
        }
    }

    public void Connect(string url, int port) 
    {
        try
        {
            socket.Connect(url, port);
            Console.WriteLine("Подключение прошло успешно: {0}", socket.Connected);
        }
        catch (SocketException ex) 
        {
            Console.WriteLine("Не удалось соединиться с сервисом: {0}", ex.Message);
        }
    } // соединяет с сервисом

    public void Dispose() 
    {
        socket.Close();
    }// закрывает сокет
}
