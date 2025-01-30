using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Net.WebSockets;
using WebSocketServer.Database;
using WebSocketServer.Interface;
using WebSocketServer.Service;
using System.Net;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<DBC>(options => options.UseNpgsql(connectionString));
builder.WebHost.UseUrls(builder.Configuration["WebHost:Address"]);
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IRabbitListenerService, RabbitListenerService>();
builder.Services.AddScoped<ISocketService, SocketService>();
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

app.UseWebSockets();
app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();
        while (true) 
        {
            List<string> message = rabbitService.GetMessages();
            string mess = "";
            if (message.Count() > 0) 
            {
                mess = message[0];
                message.Clear();
                rabbitService.ClearList();
            }
            var bytes = Encoding.UTF8.GetBytes(mess);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);
            if(ws.State == WebSocketState.Open) 
            {
                await ws.SendAsync(arraySegment,
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
            else if (ws.State == WebSocketState.Closed || ws.State == WebSocketState.Aborted) 
            {
                break;
            }
            Thread.Sleep(1000);
        }
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
