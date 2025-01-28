using WebSocketServer.Schems;

namespace WebSocketServer.Interface;

public interface IRabbitListenerService
{
    public void ListenQueue();

    public List<string> GetMessages();

}
