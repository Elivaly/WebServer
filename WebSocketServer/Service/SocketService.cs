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
    public SocketService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Accept() // получает входящее подключенние
    { 
        socket.Accept();
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
            Console.WriteLine("Попытка подключения к сервису...");
            socket.Connect(url, port);
            Console.WriteLine("Подключение прошло успешно: {0}", socket.Connected, socket.LocalEndPoint);
        }
        catch (SocketException ex) 
        {
            Console.WriteLine("Не удалось соединиться с сервисом: {0}", ex.Message);
        }
    } // соединяет с сервисом

    public void Dispose() 
    {
        Console.WriteLine("Идет закрытие соединения...");
        socket.Close();
        socket.Dispose();
        Console.WriteLine("Соединение открыто: {0}", socket.Connected);
    }// закрывает сокет
}
