using System.IO;
using System.Net;
using System.Net.Sockets;


namespace WebSocketServer.Service;

public class SocketService
{
    Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
    private bool IsConnected;
    private int port = 5001;
    private string url = "192.168.5.32";
    public SocketService() 
    {
        
    }

    public void Bind(IPEndPoint endPoint) 
    {
        IPAddress ip = IPAddress.Parse(url);
        IPEndPoint ep = new IPEndPoint(ip, port);
        socket.Bind(ep); 
    }

    public void Listen(IPAddress address, int port) 
    {
        if(!IsConnected) 
        {
            if(socket == null) 
            {
                socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            }
            socket.Bind(new IPEndPoint(address, port));
            socket.Listen(1);
        }
    }

    public void Connect(string url, int port) 
    {
        socket.Connect(url, port);
    }

    public void Dispose() 
    {
        socket.Close();
    }
}
