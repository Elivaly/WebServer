using AuthService.Handler;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TokenController : BaseController
{
    IConfiguration _configuration;

    public TokenController(IConfiguration configuration) : base(configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Проверка времени жизни токена
    /// </summary>
    /// <remarks>
    /// Возвращает время жизни токена
    /// </remarks>
    /// <response code="400">Некорректные данные о токене</response>
    /// <response code="401">Время жизни токена истекло</response>
    /// <response code="404">Пользователь не существует</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("[action]")]
    public IActionResult CheckTokenTime()
    {
        int time = GetRemainingSeconds();
        if (time == -2) return BadRequest("Возникла ошибка во время валидации токена");
        else if (time == 0) return NotFound("Пользователь не существует");
        return Ok(new { remainingTime = time, statusCode = 200 });
    }

    /// <summary>
    /// Обновление времени жизни токена
    /// </summary>
    /// <remarks>
    /// Возвращает токен с обновленным временем жизни
    /// </remarks>
    /// <response code="401">Некорректные данные о токене</response>
    /// <response code="404">Пользователь не существует</response>
    /// <response code="500">Во время исполнения произошла ошибка на стороне сервера</response>
    [HttpPost]
    [Route("[action]")]
    public IActionResult RefreshTokenTime()
    {
        string token = RefreshTime();
        return Ok(new { message = token, statusCode = 200 });
    }

    /// <summary>
    /// Получение данных о пользователе
    /// </summary>
    /// <remarks>
    /// Возвращает хэш и логин пользователя
    /// </remarks>
    /// <response code="400">Некорректные данные о токене</response>
    /// <response code="401">Время жизни токена истекло</response>
    /// <response code="404">Пользователь не существует</response>
    /// <response code="500">Во время исполенения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("[action]")]
    public IActionResult GetUserData()
    {
        var id = GetID();
        using (DBC db = new DBC(_configuration))
        {
            var user = db.Users.FirstOrDefault(u => u.ID == id);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не существует", statusCode = 404 });
            }
            return Ok(new { username = user.Username, passwordHash = user.Password, statusCode = 200 });
        }
    }

}
