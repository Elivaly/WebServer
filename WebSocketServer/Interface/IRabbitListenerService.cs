namespace WebSocketServer.Interface;

public interface IRabbitListenerService
{
    public List<string> ListenQueue();
}
