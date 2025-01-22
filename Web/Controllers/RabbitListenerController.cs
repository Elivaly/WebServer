using System.Runtime.CompilerServices;
using AuthService.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers;

[Route("/api/[controller]")]
[ApiController]

public class RabbitListenerController : Controller
{
    private readonly IRabbitListenerService _rabbitListenerService;
    private readonly IConfiguration _configuration;

    public RabbitListenerController(IConfiguration configuration, IRabbitListenerService rabbitListenerService) 
    {
        _rabbitListenerService = rabbitListenerService;
        _configuration = configuration;
    }

    /// <summary>
    /// Прочитать очередь (в разработке)
    /// </summary>
    [Route("[action]")]
    [HttpGet]
    public IActionResult ReceiveMessage() 
    {
        _rabbitListenerService.ListenQueue();
         return Ok("Прослушивается очередь...");
    }
}
