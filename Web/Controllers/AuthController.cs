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
    /// Web -> Controllers -> AuthController
    /// </remarks>
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
           
            var token = GenerateJwtToken(existingUser);
            _configuration["JWT:Token"]=token;
            Response.Headers.Add("Authorization", $"Bearer {token}");

            HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });

            return Ok(new { token = token});
        }
    }

    /// <summary>
    /// Вход по ЭЦП
    /// </summary>
    /// <remarks>
    /// Web -> Controllers -> AuthController
    /// </remarks>
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
    /// Выход из аккаунта, удаляет токен
    /// </summary>
    /// <remarks>
    /// Web -> Controllers -> AuthController
    /// </remarks>
    [HttpPost]
    [Route("Logout")]
    public IActionResult Loguot() 
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

        Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        HttpContext.Response.Cookies.Delete("jwtToken");
        _configuration["JWT:Token"] = null;
        return Ok(new { message = "Пользователь вышел из системы" }); 
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
            new Claim("role", user.role),
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
