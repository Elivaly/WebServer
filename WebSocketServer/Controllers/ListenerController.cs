using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WebSocketServer.Controllers;

[Route("/api/[controller]")]
public class ListenerController : Controller
{
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
