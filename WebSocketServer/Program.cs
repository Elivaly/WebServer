using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Net.WebSockets;
using WebSocketServer.Database;
using WebSocketServer.Interface;
using WebSocketServer.Service;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<DBC>(options => options.UseNpgsql(connectionString));
builder.WebHost.UseUrls(builder.Configuration["WebHost:Address"]);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IRabbitListenerService, RabbitListenerService>();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SocketServer",
        Version = "v1.1",
        Description = "Решение представляет собой сокет сервер"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.EnableAnnotations();
});
builder.Services.AddSingleton<RabbitListenerService>();
builder.Services.AddSingleton<SocketHubService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

var rabbitService = app.Services.GetRequiredService<RabbitListenerService>();
rabbitService.ListenQueue();

var socketService = app.Services.GetRequiredService<SocketHubService>();
List<WebSocket> connections = new List<WebSocket>();
app.UseWebSockets();
app.UseRouting();
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        var currentName = await socketService.GetRole();
        var currentID = await socketService.GetID();
        Console.WriteLine(currentName + " отправил сообщение...");
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        connections.Add(ws);
        await socketService.Broadcast($"{currentName} присоединился к чату", connections);
        await socketService.Broadcast($"{connections.Count} пользователей в чате", connections);
        await socketService.ReceiveMessage(ws, async (result, buffer) => 
        {
            if (result.MessageType == WebSocketMessageType.Text)
            {
                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                await socketService.Broadcast($" {currentName} {currentID}: {message}", connections);
            }
            else if(result.MessageType == WebSocketMessageType.Close || ws.State == WebSocketState.Aborted) 
            {
                connections.Remove(ws);
                await socketService.Broadcast($"{currentName} покинул чат", connections);
                await socketService.Broadcast($"{connections.Count} пользователей в чате", connections);
                await ws.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
            }
        });

        socketService.GetMessages(ws, rabbitService);
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});


app.UseSwagger();
app.UseSwaggerUI();
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<SocketHubService>("/api/Auth/Sender");

app.Run(builder.Configuration["ApplicationHost:Address"]);
