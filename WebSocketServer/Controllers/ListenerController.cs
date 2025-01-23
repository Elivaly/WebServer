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
    
    public ListenerController(IConfiguration configuration, IRabbitListenerService rabbitListener) 
    {
        _configuration = configuration;
        _rabbitListener = rabbitListener;
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
        _rabbitListener.ListenQueue(_configuration);
        return Ok("Пока все ОК");
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
