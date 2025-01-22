namespace AuthService.Interface;

public interface IRabbitListenerService
{
    public void ListenQueue(Object obj);
    public void ListenQueue();
}
