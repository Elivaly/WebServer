using AuthService.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class RabbitController : Controller
{
    private readonly IRabbitService _rabbitService;

    public RabbitController(IRabbitService rabbitService)
    {
        _rabbitService = rabbitService;
    }

    [Route("[action]/{message}")]
    [HttpGet]
    public IActionResult SendMessage(string message) 
    {
        _rabbitService.SendMessage(message);

        return Ok(new { message = "Сообщение отправлено"});
    }
}
