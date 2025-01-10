using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthService.Handler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using AuthService.Schems;
using System.Security.Claims;
using Microsoft.AspNetCore.Localization;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using System.Security.Cryptography;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace AuthService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    IConfiguration _configuration;

    public AuthController(IConfiguration configuration)
    {
        _configuration = configuration;
        
    }

    /// <summary>
    /// Вход по логин-паролю
    /// </summary>
    /// <remarks>
    /// Выдает 2 токена для пользователя
    /// </remarks>
    /// <response code="400">Некорректно введенные данные</response>
    /// <response code="401">Неверный логин или пароль</response>
    /// <response code="500">В процессе выполнения произошла внутрисерверная ошибка</response>
    [HttpPost]
    [Route("LoginByPassword")] 
    public IActionResult LoginByPassword([FromBody] User user) 
    {

        // Проверка доступности HttpContext
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

        using (DBC db = new (_configuration)) 
        {
            user.password = Hash(user.password);
            var existingUser = db.users.FirstOrDefault(u => u.name == user.name && u.password == user.password);
            if (existingUser == null) 
            {
                return Unauthorized(new { message = "Неверный логин или пароль" });
            }
            var expiration = DateTime.UtcNow.AddMinutes(1);
            var refreshToken = GetRefreshToken();
            existingUser.expiresaccess = expiration;
            existingUser.refreshtoken = refreshToken;
            existingUser.expiresrefresh = DateTime.UtcNow.AddMinutes(2);
            db.SaveChanges();
            var token = GenerateJwtToken(existingUser);
            _configuration["JWT:Token"]=token;
            _configuration["JWT:Refresh"] = refreshToken;
            Response.Headers.Add("Authorization", $"Bearer {token}");

            HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });

            return Ok(new { access = token, refresh = refreshToken});
        }
    }

    /// <summary>
    /// Вход по ЭЦП (в разработке)
    /// </summary>
    /// <remarks>
    /// Нужен клиент и ключ получить сперва
    /// </remarks>
    /// <response code="401">Не удается подтвердить подпись</response>
    /// <response code="500">В процессе выполнения произошла внутрисерверная ошибка</response>
    [HttpPost]
    [Route("LoginByEDS")]
    public IActionResult LoginByEDS() 
    {
        // Проверка доступности HttpContext
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

        return Ok();
    }

    /// <summary>
    /// Выход из аккаунта
    /// </summary>
    /// <remarks>
    /// Убирает у пользователя оба токена
    /// </remarks>
    /// <response code="401">Пользователь не авторизован</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpPost]
    [Route("Logout")]
    public IActionResult Logout() 
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

        if(_configuration["JWT:Refresh"] == "" || _configuration["JWT:Token"] == "") 
        {
            return Unauthorized("Пользователь не вошел в систему");
        }

        using (DBC db = new DBC(_configuration))
        {
            var user = db.users.FirstOrDefault(u => u.refreshtoken == _configuration["JWT:Refresh"]);
            if (user != null) 
            {
                user.refreshtoken = "EXPIRES_DATA"; 
                db.SaveChanges();
            }
        }
        Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        HttpContext.Response.Cookies.Delete("jwtToken");
        _configuration["JWT:Token"] = "";
        _configuration["JWT:Refresh"] = "";
        return Ok(new { message = "Пользователь вышел из системы" }); 
    }

    private string GetRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
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
            new Claim(JwtRegisteredClaimNames.Sub, user.name),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var token = new JwtSecurityToken( 
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddMinutes(1),
            signingCredentials: credentials); 
        return new JwtSecurityTokenHandler().WriteToken(token); 
    }

    private string Hash(string password)
    {
        byte[] data = Encoding.Default.GetBytes(password);
        SHA1 sha = new SHA1CryptoServiceProvider();
        byte[] result = sha.ComputeHash(data);
        password = Convert.ToBase64String(result);
        return password;
    }
}
