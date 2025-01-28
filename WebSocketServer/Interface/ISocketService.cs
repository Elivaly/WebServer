using System.Net;
using System.Net.Sockets;

namespace WebSocketServer.Interface;

public interface ISocketService
{
    List<string> Listen(IPAddress adress); // прослушивание на наличие подключений
    void Close(); // закрытие сокета
    bool CheckSocketConnection(Socket socket);
}
