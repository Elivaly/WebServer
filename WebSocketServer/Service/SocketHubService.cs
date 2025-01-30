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
        await this.Clients.All.SendAsync("Receive", message, Context.ConnectionId);    
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

    public async Task GetMessages(WebSocket item, RabbitListenerService rabbit) 
    {
        while (true)
        {
            List<string> message = rabbit.GetMessages();
            string mess = "";
            if (message.Count() > 0)
            {
                mess = message[0];
                message.Clear();
                rabbit.ClearList();
            }
            var bytes = Encoding.UTF8.GetBytes(mess);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            if (item.State == WebSocketState.Open)
            {
                await item.SendAsync(arraySegment,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            else if (item.State == WebSocketState.Closed || item.State == WebSocketState.Aborted)
            {
                break;
            }
        }
        Thread.Sleep(1000);
    }

    public async Task GetRole(RabbitListenerService rabbit)
    {
        List<string> role = rabbit.GetRoleName();
        string mess = "";
        if (role.Count() > 0)
        {
            mess = role[0];
            role.Clear();
            rabbit.ClearList();
        }
    }
}
