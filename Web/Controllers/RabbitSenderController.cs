using AuthService.Handler;
using AuthService.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RabbitSenderController : Controller
{
    private readonly IRabbitSenderService _rabbitService;
    private readonly IConfiguration _configuration;

    public RabbitSenderController(IRabbitSenderService rabbitService, IConfiguration configuration)
    {
        _rabbitService = rabbitService;
        _configuration = configuration;
    }

    /// <summary>
    /// Отправить сообщение (в разработке)
    /// </summary>
    /// <remarks>
    /// Отсылает сообщение в очередь
    /// </remarks>
    [Route("[action]")]
    [HttpGet]
    public IActionResult SendMessage(string message) 
    {
        _rabbitService.SendMessage(message);

        using (DBC db = new DBC(_configuration)) 
        {
                
        }

        return Ok(new { message = "Сообщение отправлено" });
    }
}
