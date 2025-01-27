using AuthService.Handler;
using AuthService.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using AuthService.Schems;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SenderController : Controller
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
    [Route("[action]")]
    [HttpPost]
    public IActionResult SendMessage(Message message) 
    {
        using (DBC db = new DBC(_configuration)) 
        {
            message.Datetime_Create = DateTime.UtcNow;
            message.ID_User = GetID();
            if(message.ID_User == 0) 
            {
                return Unauthorized(new { message = "Пользователь не вошел в систему", StatusCode = 401 });
            }
            db.Messages.Add(message);
            db.SaveChanges();
        }
        _rabbitService.SendMessage(message);

        return Ok(new { message = "Сообщение отправлено", StatusCode = 200 });
    }

    private int GetID() 
    {
        var key = Encoding.ASCII.GetBytes(_configuration["JWT:Key"]);
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };
            handler.ValidateToken(_configuration["JWT:Token"], tokenValidationParameters, out SecurityToken validatedToken);
            var jwt = validatedToken as JwtSecurityToken;
            var id = jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            // Проверка наличия пользователя в базе данных
            using (DBC db = new DBC(_configuration))
            {
                var expiration = jwt.ValidTo;
                var timeRemaining = expiration - DateTime.UtcNow;
                var timeRemainingMilliSeconds = (int)timeRemaining.TotalMilliseconds;
                if (timeRemainingMilliSeconds < 0)
                {
                    return 0;
                }
                var user = db.Users.FirstOrDefault(u => u.ID == int.Parse(id));
                if (user == null)
                {
                    return 0;
                }
                return user.ID;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return 0;
        }
    }
}
