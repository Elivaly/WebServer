using System.Net;

namespace WebSocketServer.Interface;

public interface ISocketService
{
    void Listen(IPAddress adress, int port); // прослушивание на наличие подключений
    void Connect(string url, int port); // соединение с сервисом
    void Close(); // закрытие сокета
}
