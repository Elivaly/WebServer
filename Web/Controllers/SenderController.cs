using AuthService.Handler;
using AuthService.Interface;
using AuthService.Schems;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SenderController : BaseController
{
    private readonly IRabbitSenderService _rabbitService;
    private readonly IConfiguration _configuration;

    public SenderController(IRabbitSenderService rabbitService, IConfiguration configuration) : base(configuration)
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
        int id = GetID();

        using (DBC db = new DBC(_configuration))
        {
            message.Datetime_Create = DateTime.UtcNow;
            message.ID_User = id;
            if (message.ID_User == 0)
            {
                return Unauthorized(new { message = "Пользователь не вошел в систему", statusCode = 401 });
            }
            db.Messages.Add(message);
            db.SaveChanges();
        }
        _rabbitService.SendMessage(message);

        return Ok(new { message = "Сообщение отправлено", statusCode = 200 });
    }


}
