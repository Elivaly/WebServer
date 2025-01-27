using System.Net;
using System.Net.Sockets;

namespace WebSocketServer.Interface;

public interface ISocketService
{
    void Listen(IPAddress adress); // прослушивание на наличие подключений
    void Connect(string url, int port); // соединение с сервисом
    void Close(); // закрытие сокета
    bool CheckSocketConnection(Socket socket);
}
