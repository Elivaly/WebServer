using Microsoft.AspNetCore.Mvc;
using WebSocketServer.Interface;

namespace WebSocketServer.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class ListenerController : Controller
{
    IRabbitListenerService _rabbitListener;

    public ListenerController(IRabbitListenerService rabbitListener)
    {
        _rabbitListener = rabbitListener;
    }

    /// <summary>
    /// Прослушать очередь
    /// </summary>
    /// <remarks>
    /// Прослушивает очередь на наличие сообщений и подключений
    /// </remarks>
    /// <response code = "404">Отсутствуют сообщения</response>
    [HttpGet]
    [Route("[action]")]
    public IActionResult ListenQueue()
    {
        _rabbitListener.ListenQueue();
        List<string> messages = _rabbitListener.GetMessages();
        _rabbitListener.ClearList();
        int colMessages = messages.Count;
        string text;
        if (messages.Count == 0)
        {
            return NotFound(new { message = "В очереди нет сообщений", statusCode = 404 });
        }
        if (messages.Count > 1)
        {
            string textMessages = string.Join(", ", messages);
            text = $"Вам пришли новые сообщения.\nКоличество сообщений: {colMessages}\nСообщения: {textMessages}";
            return Ok(text);
        }
        else
        {
            string textMessages = string.Join(", ", messages);
            text = $"Вам пришло новое сообщение.\nСообщение: {textMessages}";
            return Ok(text);
        }
    }

}
