using System.Net;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebSocketServer.Interface;
using WebSocketServer.Service;

namespace WebSocketServer.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ListenerController : Controller
{
    IConfiguration _configuration;
    IRabbitListenerService _rabbitListener;
    ISocketService _socketService;
    
    public ListenerController(IConfiguration configuration, IRabbitListenerService rabbitListener, ISocketService socketService) 
    {
        _configuration = configuration;
        _rabbitListener = rabbitListener;
        _socketService = socketService;
    }

    /// <summary>
    /// Прослушать очередь
    /// </summary>
    /// <remarks>
    /// Прослушивает очередь на наличие сообщений и подключений
    /// </remarks>
    [HttpGet]
    [Route("[action]")]
    public IActionResult ListenQueue()
    {
        //_socketService.Connect(_configuration["SocketSettings:Url"], int.Parse(_configuration["SocketSettings:ServicePort"]));

        List<string> messages = _socketService.Listen(IPAddress.Parse(_configuration["SocketSettings:Url"])); 
        int colMessages = messages.Count;
        string text;
        if (messages.Count > 1)
        {
            string textMessages = string.Join(", ", messages);
            text = $"Вам пришли новые сообщения.\nКоличество сообщений: {colMessages}\nСообщения: {textMessages}";
            return Ok(text);
        }
        else
        {
            string textMessages = string.Join(", ", messages);
            text = $"Вам пришло новое сообщение.\nКоличество сообщений: {colMessages}\nСообщение: {textMessages}";
            return Ok(text);
        }
    }

    /// <summary>
    /// Получить время сервера
    /// </summary>
    /// <remarks>
    /// Возвращает время на машине
    /// </remarks>
    [HttpGet]
    [Route("[action]")]
    public IActionResult GetServerTime() 
    {
        var time = DateTime.Now;
        return Ok(new { ServerTime = time.ToString()});
    }

}
