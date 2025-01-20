namespace AuthService.Interface;

public interface IRabbitService
{
    public void SendMessage(Object obj);
    public void SendMessage(string message);
}
