using AuthService.Handler;
using AuthService.Interface;
using AuthService.Schemas;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SenderController : ControllerBase
{
    private readonly IRabbitSenderService _rabbitService;
    private readonly IConfiguration _configuration;

    public SenderController(IRabbitSenderService rabbitService, IConfiguration configuration)
    {
        _rabbitService = rabbitService;
        _configuration = configuration;
    }

    /// <summary>
    /// Отправить сообщение (в разработке)
    /// </summary>
    /// <remarks>
    /// Отсылает сообщение в очередь и БД
    /// </remarks>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="500">Во время исполнения произошла ошибка на стороне сервера</response>
    [Route("[action]")]
    [HttpPost]
    public IActionResult SendMessage(Message message)
    {
        using (DBC db = new DBC(_configuration))
        {
            message.Datetime_Create = DateTime.UtcNow;
            db.Messages.Add(message);
            db.SaveChanges();
        }
        _rabbitService.SendMessage(message);

        return Ok(new { message = "Сообщение отправлено", statusCode = 200 });
    }


}
