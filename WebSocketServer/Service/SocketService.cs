using System.IO;
using System.Net;
using System.Net.Sockets;


namespace WebSocketServer.Service;

public class SocketService
{
    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    public SocketService() 
    {
        
    }

    public void Bind() 
    {
        var port = 5001;
        var url = "192.168.5.32";
        IPAddress ip = IPAddress.Parse(url);
        IPEndPoint ep = new IPEndPoint(ip, port);
        socket.Bind(ep); 
        socket.Connect(url, 5000);
    }

    public void Close() 
    {
        socket.Close();
    }
}
