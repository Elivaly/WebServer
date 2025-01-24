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

    public async void Listen(IPAddress address, int port) // слушает на постоянной основе есть ли подключения
    {
        //CheckSocket();
        //while (IsConnected)
        //{
        //    listen_socket.Listen(1);
        //    Console.WriteLine("Сокет слушает подключения");
        //    //await listen_socket.BeginAccept();
        //    Console.WriteLine("Сокет начинает принимать сообщения");
        //    listen_socket.Close();
        //    Console.WriteLine("Сокет завершил прослушивание и закрыл соединение");
        //}

        Socket listen_socket = null;
        try
        {
            listen_socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            IPEndPoint endPoint = new IPEndPoint(IPAddress.Parse(_configuration["SocketSettings:Url"]), 5001);
            listen_socket.Bind(endPoint);
            listen_socket.Listen(1);

            var handler = await listen_socket.AcceptAsync();
            while (true)
            {
                var buffer = new byte[4096];
                var received = await handler.ReceiveAsync(buffer, SocketFlags.None);
                var response = Encoding.UTF8.GetString(buffer, 0, received);

                var eom = "<|ПОЛУЧАТЕЛЬ|>";
                if (response.IndexOf(eom) > -1 /* is end of message */)
                {
                    Console.WriteLine(
                        $"Socket server received message: \"{response.Replace(eom, "")}\"");

                    var ackMessage = "<|ОПРАШИВАТЕЛЬ|>";
                    var echoBytes = Encoding.UTF8.GetBytes(ackMessage);
                    await handler.SendAsync(echoBytes, 0);
                    Console.WriteLine(
                        $"Socket server sent acknowledgment: \"{ackMessage}\"");

                    break;
                }
            }
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("Socket object was disposed. Creating a new instance.");
            listen_socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            // Повторите настройку и прослушивание сокета
        }
    }

    public void CheckSocket()
    {
        if (socket == null) socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        if (!socket.Connected)
        {
            socket.Connect(_configuration["SocketSettings:Address"], 5000);
            IsConnected = socket.Connected;
        }
        if (listen_socket == null) listen_socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0);
        try
        {
            if (listen_socket.LocalEndPoint == null)
            {
                listen_socket.Bind(endPoint);
            }
        }
        catch (Exception ex) 
        {
            Console.WriteLine(ex.StackTrace);   
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
