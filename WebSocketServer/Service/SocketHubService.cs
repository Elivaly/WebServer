using Microsoft.AspNetCore.SignalR;

namespace WebSocketServer.Service;

public class SocketHubService : Hub
{
    IConfiguration _configuration;

    public SocketHubService(IConfiguration configuration) 
    {
        _configuration = configuration;
    }
    public async void Send(string message) 
    {
        await this.Clients.All.SendAsync("Получено сообщение ", message);    
    }
}
