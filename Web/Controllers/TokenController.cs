using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection.Metadata.Ecma335;
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
    /// Возвращает айдишник пользователя
    /// </remarks>
    /// <response code="400">Некорректные данные о токене</response>
    /// <response code="401">Время жизни токена истекло</response>
    /// <response code="404">Пользователь не существует</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("DecodeToken")]
    public IActionResult DecodeToken([Required] string token)
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

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
            handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken); 
            var jwt = validatedToken as JwtSecurityToken; 
            var id = jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            // Проверка наличия пользователя в базе данных
            using (DBC db = new DBC(_configuration)) 
            {
                if (token != _configuration["JWT:Token"])
                {
                    return Unauthorized("Время жизни токена истекло");
                }
                var expiration = jwt.ValidTo;
                var timeRemaining = expiration - DateTime.UtcNow;
                var timeRemainingMilliSeconds = (int)timeRemaining.TotalMilliseconds;
                if (timeRemainingMilliSeconds < 0)
                {
                    return Unauthorized(new { message = "Время жизни токена истекло" });
                }
                var user = db.Users.FirstOrDefault(u => u.Id == int.Parse(id));
                if (user == null)
                {
                    return NotFound("Пользователь не существует");
                }
                return Ok(new { ID = user.Id });
            }
        } 
        catch (Exception ex)
        { 
            Console.WriteLine($"Ошибка: {ex.Message}");
            return BadRequest("Некорректные данные о токене");
        }
    }

    /// <summary>
    /// Проверка времени жизни токена
    /// </summary>
    /// <remarks>
    /// Возвращает время жизни токена
    /// </remarks>
    /// <response code="400">Некорректные данные о токене</response>
    /// <response code="401">Срок жизни токена истек</response>
    /// <response code="404">Пользователь не существует</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("CheckTokenTime")]
    public IActionResult CheckTokenTime([Required]string token)
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

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
            handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            var jwt = validatedToken as JwtSecurityToken;
            var id = jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            // Проверка наличия пользователя в базе данных
            using (DBC db = new DBC(_configuration))
            {
                if (token != _configuration["JWT:Token"])
                {
                    return Unauthorized("Срок жизни токена истек");
                }
                var expiration = jwt.ValidTo;
                var timeRemaining = expiration - DateTime.UtcNow;
                var timeRemainingMilliSeconds = (int)timeRemaining.TotalMilliseconds;
                if (timeRemainingMilliSeconds < 0)
                {
                    return Ok(new { timeRemaining = -1});
                }
                var user = db.Users.FirstOrDefault(u => u.Id == int.Parse(id));
                if (user == null)
                {
                    return NotFound("Пользователь не существует");
                }
                return Ok(new { timeRemaining = timeRemainingMilliSeconds });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return BadRequest("Некорректные данные о токене");
        }
    }


    /// <summary>
    /// Подтверждение действительности токена
    /// </summary>
    /// <remarks>
    /// Возвращает булевое значение (есть токен или нет)
    /// </remarks>
    /// <response code="400">Некорректные данные о токене</response>
    /// <response code="401">Время жизни токена истекло</response>
    /// <response code="404">Пользователь не существует</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("SubmitJWT")]
    public IActionResult SubmitJWT([Required]string token) 
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

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
            handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            var jwt = validatedToken as JwtSecurityToken;
            var id = jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            // Проверка наличия пользователя в базе данных
            using (DBC db = new DBC(_configuration))
            {
                if (token != _configuration["JWT:Token"])
                {
                    return Unauthorized("Время жизни токена истекло");
                }
                var expiration = jwt.ValidTo;
                var timeRemaining = expiration - DateTime.UtcNow;
                var timeRemainingMilliSeconds = (int)timeRemaining.TotalMilliseconds;
                if (timeRemainingMilliSeconds < 0)
                {
                    return Unauthorized(new { message = "Время жизни токена истекло" });
                }
                var user = db.Users.FirstOrDefault(u => u.Id == int.Parse(id));
                if (user == null)
                {
                    return NotFound("Пользователь не существует");
                }
                return Ok(true);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return BadRequest("Некорректные данные о токене");
        }
    }


    /// <summary>
    /// Обновление времени жизни токена
    /// </summary>
    /// <remarks>
    /// Возвращает новый токен с новым временем жизни
    /// </remarks>
    /// <response code="400">Некорректные данные о токене</response>
    /// <response code="404">Пользователь не существует</response>
    /// <response code="500">Во время исполнения произошла ошибка на стороне сервера</response>
    [HttpPost]
    [Route("RefreshTokenTime")]
    public IActionResult RefreshTokenTime([Required]string token) 
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

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
            handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            var jwt = validatedToken as JwtSecurityToken;
            var id = jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            // Проверка наличия пользователя в базе данных
            using (DBC db = new DBC(_configuration))
            {
                var user = db.Users.FirstOrDefault(u => u.Id == int.Parse(id));
                if (user == null)
                {
                    return NotFound("Пользователь не существует");
                }
                var data = GetDataFromExpiredToken(token);
                var newToken = GenerateJwtToken(data);
                _configuration["JWT:Token"] = newToken;
                HttpContext.Response.Cookies.Append("jwtToken", newToken, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });
                Response.Headers.Add("Authorization", $"Bearer {newToken}");
                return Ok(new { token = newToken });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка:{ex.Message}");
            return BadRequest("Некорректные данные о токене");
        }
    }

    /// <summary>
    /// Получение хеша и логина пользователя
    /// </summary>
    /// <remarks>
    /// Если токен действителен, то вернет айдишник пользователя
    /// </remarks>
    /// <response code="400">Некорректные данные о токене</response>
    /// <response code="401">Время жизни токена истекло</response>
    /// <response code="404">Пользователь не существует</response>
    /// <response code="500">Во время исполенения произошла внутрисерверная ошибка</response>
    [HttpGet]
    [Route("GetUserData")]
    public IActionResult GetUserData([Required] string token) 
    {
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

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
            handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
            var jwt = validatedToken as JwtSecurityToken;
            var id = jwt.Claims.First(claim => claim.Type == JwtRegisteredClaimNames.Sub).Value;
            // Проверка наличия пользователя в базе данных
            using (DBC db = new DBC(_configuration))
            {
                if (token != _configuration["JWT:Token"])
                {
                    return Unauthorized("Время жизни токена истекло");
                }
                var expiration = jwt.ValidTo;
                var timeRemaining = expiration - DateTime.UtcNow;
                var timeRemainingMilliSeconds = (int)timeRemaining.TotalMilliseconds;
                if (timeRemainingMilliSeconds < 0)
                {
                    return Unauthorized(new { message = "Время жизни токена истекло" });
                }
                var user = db.Users.FirstOrDefault(u => u.Id == int.Parse(id));
                if (user == null)
                {
                    return NotFound("Пользователь не существует");
                }
                return Ok(new {username = user.Name, passwordHash = user.Password});
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return BadRequest("Некорректные данные о токене");
        }
    }


    private ClaimsPrincipal GetDataFromExpiredToken(string token)
    {
        var key = _configuration["JWT:Key"];
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key), "JWT Key cannot be null or empty.");
        }

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = false,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["JWT:Issuer"],
            ValidAudience = _configuration["JWT:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var data = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
        var jwtToken = securityToken as JwtSecurityToken;
        if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Invalid token");
        }

        return data;
    }

    private string GenerateJwtToken(ClaimsPrincipal data)
    {
        var key = _configuration["JWT:Key"];
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentNullException(nameof(key), "JWT Key cannot be null or empty.");
        }
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, data.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown") // возвращает unknown после рефреша
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"],
            expires: DateTime.Now.AddMinutes(1),
            claims: claims,
            signingCredentials: credentials);
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString);
    }


    /// <summary>
    /// Получение времени сервера
    /// </summary>
    /// <remarks>
    /// Возвращает текущее время на машине
    /// </remarks>
    [HttpGet]
    [Route("GetServerTime")]
    public IActionResult GetServerTime() 
    {
        var time = DateTime.Now;
        return Ok(new { ServerTime = time });
    } 

}
