using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AuthService.Handler;
using AuthService.Schems;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TokenController : ControllerBase
{
    IConfiguration _configuration;
    
    public TokenController(IConfiguration configuration) 
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Декодировка токена
    /// </summary>
    /// <remarks>
    /// Для перепроверки поступают ли пользовательские данные во время создания токена
    /// </remarks>
    /// <response code="404">Токен отсутствует</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("DecodeToken")]
    [Obsolete]
    public IActionResult DecodeToken()
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

        var token = HttpContext.Request.Cookies["jwtToken"];
        if (_configuration["JWT:Token"] == "" || token == null) 
        {
            return NotFound(new { message = "Время жизни токена истекло" });
        }
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token); 
        var expiration = jwtToken.ValidTo;
        var audience = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Aud)?.Value;
        var issuer = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value;
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value; 
        return Ok(new { Exp = expiration, Audience = audience, Issuer = issuer, Username = username});
    }

    /// <summary>
    /// Проверка времени жизни токена
    /// </summary>
    /// <remarks>
    /// Возвращает время жизни токена
    /// </remarks>
    /// <response code="401">Срок жизни токена истек</response>
    /// <response code="404">Токен отсутствует</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("CheckTokenTime")]
    public IActionResult CheckTokenTime()
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

        var token = _configuration["JWT:Token"];
        if (string.IsNullOrEmpty(token))
        {
            return NotFound(new { message = "Токен отсутствует" });
        }
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expiration = jwtToken.ValidTo;
        var timeRemaining = expiration - DateTime.UtcNow;
        var timeRemainingMilliSeconds = (int)timeRemaining.TotalMilliseconds;
        if (timeRemainingMilliSeconds < 0) 
        {
            return Ok(new { timeRemaining = -1 });
        }
        return Ok(new { timeRemaining = timeRemainingMilliSeconds });
    }

    /// <summary>
    /// Обновление времени жизни токена
    /// </summary>
    /// <remarks>
    /// Перезапускает таймлайн если рефреш токен жив.
    /// </remarks>
    /// <response code="400">Неккоректные данные</response>
    /// <response code="401">Время жизни токена истекло. Пользователь не авторизован</response>
    /// <response code="404">Токен отсутствует</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpPost]
    [Route("RefreshTokenTime")]
    public IActionResult RefreshTokenTime([Required] string refreshToken)
    {
        if (HttpContext == null) 
        { 
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");
        var tokenA = "";
        var tokenR = "";
        using (DBC db = new DBC(_configuration)) 
        {
            var user = db.Users.FirstOrDefault(u => u.RefreshToken == refreshToken);
            if (user == null) 
            {
                return BadRequest("Некорректные данные");
            }
 
            if (DateTime.Now > user.ExpiresRefresh) 
            {
                _configuration["JWT:Token"] = "";
                _configuration["JWT:Refresh"] = "";
                user.RefreshToken = "EXPIRES_DATA";
                return Unauthorized("Время жизни рефреш токена истекло. Перепройдите авторизацию");
            }
            var accessToken = GenerateJwtToken(user);
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(accessToken);
            var expiration = jwtToken.ValidTo;
            user.ExpiresAccess = expiration;
            tokenA = accessToken;
            tokenR = GetRefreshToken();
            user.RefreshToken = tokenR;
            user.ExpiresRefresh = DateTime.UtcNow.AddMinutes(2);
            db.SaveChanges();
        }
        _configuration["JWT:Token"] = tokenA;
        _configuration["JWT:Refresh"] = tokenR;
        HttpContext.Response.Cookies.Append("jwtToken", tokenA, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });
        Response.Headers.Add("Authorization", $"Bearer {tokenA}");

        return Ok(new { access = tokenA, refresh = tokenR });

    }

    /// <summary>
    /// Подтверждение действительности токена
    /// </summary>
    /// <remarks>
    /// Возвращает булевое значение (есть токен или нет)
    /// </remarks>
    /// <response code="401">Время жизни токена истекло</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("SubmitJWT")]
    public IActionResult SubmitJWT() 
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");
        var token = _configuration["JWT:Token"];
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { message = "Время жизни токена истекло" });
        }
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expiration = jwtToken.ValidTo;
        var timeRemaining = expiration - DateTime.UtcNow;
        if (timeRemaining <= TimeSpan.Zero)
        {
            return Unauthorized(new { message = "Время жизни токена истекло" });
        }
        return Ok(true);
    }

    /// <summary>
    /// Полчение времени сервера
    /// </summary>
    [HttpGet]
    [Route("GetServerTime")]
    public IActionResult GetServerTime() 
    {
        var time = DateTime.Now;
        return Ok(new { ServerTime = time });
    } 

    private string GenerateJwtToken(User user)
    {

        var key = _configuration["JWT:Key"];
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key), "JWT Key cannot be null or empty.");
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            expires: DateTime.Now.AddMinutes(1),
            claims: claims,
            signingCredentials: credentials);
        return new JwtSecurityTokenHandler().WriteToken(token);

    }

    private string GetRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

}
