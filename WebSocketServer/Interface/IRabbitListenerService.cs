using WebSocketServer.Schems;

namespace WebSocketServer.Interface;

public interface IRabbitListenerService
{
    public void ListenQueue();

    public List<string> GetMessages();
    public List<string> GetRoleName();

    public void CloseConnection();
    public void ClearList();

}
