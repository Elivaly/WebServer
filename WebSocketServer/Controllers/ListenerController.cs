using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebSocketServer.Service;

namespace WebSocketServer.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ListenerController : Controller
{
    IConfiguration _configuration;
    
    public ListenerController(IConfiguration configuration) 
    {
        _configuration = configuration;
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
        return Ok();
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
