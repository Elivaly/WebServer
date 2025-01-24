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
    private bool IsConnected;
    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    Socket listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse("192.168.5.32"), 5001);
    public SocketService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void Accept() // получает входящее подключенние
    { 
        socket.Accept();
    }

    public async void Listen(IPAddress address, int port) // слушает на постоянной основе есть ли подключения
    {
        while (IsConnected)
        {
            CheckSocket();
            listen_socket.Listen(1);
            await listen_socket.BeginAccept();
        }
    }

    public void CheckSocket()
    {
        if (socket == null) socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        if (!socket.Connected)
        {
            socket.Connect(_configuration["SocketSettings:Address"], 5000);
        }
        if (listen_socket == null) listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        if (listen_socket.LocalEndPoint == null) 
        {
            listen_socket.Bind(endPoint);
        }
    }

    public void Connect(string url, int port) 
    {
        try
        {
            Console.WriteLine("Попытка подключения к сервису...");
            socket.Connect(url, 15672);
            IsConnected = socket.Connected;
            EndPoint endPoint;
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
        IsConnected = true;
        Console.WriteLine("Соединение открыто: {0}", socket.Connected);
    }// закрывает сокет
}
