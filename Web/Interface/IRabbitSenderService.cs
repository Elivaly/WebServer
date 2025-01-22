namespace AuthService.Interface;

public interface IRabbitSenderService
{
    public void SendMessage(Object obj);
    public void SendMessage(string message);
}
