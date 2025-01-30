using System.Net.WebSockets;
using Microsoft.AspNetCore.SignalR;
namespace WebSocketServer.Interface;


public interface ISocketHubService
{
    public async Task Send(string message) { }
}
