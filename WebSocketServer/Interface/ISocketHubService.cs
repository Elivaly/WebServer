using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using WebSocketServer.Service;
namespace WebSocketServer.Interface;


public interface ISocketHubService
{
    public async Task Send(string message) { }

    public async Task Broadcast(string message, List<WebSocket> connections) { }

    public async Task ReceiveMessage(WebSocket socket, Action<WebSocketReceiveResult, byte[]> handleMessage) { }

    public async Task GetMessages(WebSocket item, RabbitListenerService rabbit) { }
    public async Task GetRole(RabbitListenerService rabbit) { }
}
