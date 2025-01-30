using Microsoft.AspNetCore.SignalR;
using WebSocketServer.Interface;

namespace WebSocketServer.Service;

public class SocketHubService : Hub, ISocketHubService
{
    IConfiguration _configuration;

    public SocketHubService(IConfiguration configuration) 
    {
        _configuration = configuration;
    }
    public async void Send(string message) 
    {
        await this.Clients.All.SendAsync("Уведомление", message, Context.ConnectionId);    
    }
}
