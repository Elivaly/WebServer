namespace WebSocketServer.Interface;

public interface IRabbitListenerService
{
    public void ListenQueue(Object obj);
}
