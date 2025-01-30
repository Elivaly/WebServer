using System.Net.WebSockets;
using System.Text;
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
    public async Task Send(string message) 
    {
        await this.Clients.All.SendAsync("Уведомление", message, Context.ConnectionId);    
    }

    public async Task Broadcast(string message, List<WebSocket> connections) 
    {
        var bytes = Encoding.UTF8.GetBytes(message);
        foreach (var socket in connections) 
        {
            if(socket.State == WebSocketState.Open) 
            {
                var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
                await socket.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
    }

    public async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage) 
    {
        var buffer = new byte[4096];
        while (socket.State == WebSocketState.Open) 
        {
            var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            handleMessage(result, buffer);
        }
    }
}
