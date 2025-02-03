using Microsoft.AspNetCore.Mvc;

namespace WebSocketServer.Controllers;

[Route("/api/[controller]")]
[ApiController]
public class Test : Controller
{
    IConfiguration _configuration;

    public Test(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult GetUserID()
    {
        return Ok(new { id = int.Parse(_configuration["UserSettings:ID"]) });
    }
}
