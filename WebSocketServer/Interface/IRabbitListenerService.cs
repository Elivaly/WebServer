namespace WebSocketServer.Interface;

public interface IRabbitListenerService
{
    public void ListenQueue();

    public List<string> GetMessages();

    public void CloseConnection();
    public void ClearList();

}
