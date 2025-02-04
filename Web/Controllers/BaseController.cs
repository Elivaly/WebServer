using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Handler;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Windows.UI.Xaml;

namespace AuthService.Controllers;

[ApiController]
public class BaseController : Controller
{
    readonly IConfiguration _configuration;

    public BaseController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    protected int GetID()
    {
        string token = HttpContext.Request.Headers.Authorization;
        return GetID(token);
    }

    protected int GetRemainingSeconds()
    {
        string token = HttpContext.Request.Headers.Authorization;
        return GetRemainingSeconds(token);
    }

    protected string RefreshTime()
    {
        string token = HttpContext.Request.Headers.Authorization;
        return RefreshTime(token);
    }

    private int GetID(string token)
    {
        var key = Encoding.ASCII.GetBytes(_configuration["JWT:Key"]);
        var handler = new JwtSecurityTokenHandler();
        try
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
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
                var user = db.Users.FirstOrDefault(u => u.ID == int.Parse(id));
                if (user == null)
                {
                    return -1;
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

    private int GetRemainingSeconds(string token)
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
            handler.ValidateToken(token, tokenValidationParameters, out SecurityToken validatedToken);
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
                    return -1;
                }
                var user = db.Users.FirstOrDefault(u => u.ID == int.Parse(id));
                if (user == null)
                {
                    return 0;
                }
                return timeRemainingMilliSeconds;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
            return -2;
        }
    }

    private string RefreshTime(string token)
    {
        var message = "";
        try
        {
            var id = GetID(token);
            // Проверка наличия пользователя в базе данных
            using (DBC db = new DBC(_configuration))
            {
                var user = db.Users.FirstOrDefault(u => u.ID == id);
                if (user == null)
                {
                    message = "404";
                    return message;
                }
                var data = GetDataFromExpiredToken(token);
                var newToken = GenerateJwtToken(data);
                HttpContext.Response.Cookies.Append("jwtToken", newToken, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });
                HttpContext.Request.Headers.Authorization = newToken;
                message = newToken;
                return message;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка:{ex.Message}");
            message = "Отсутствуют данные о токене";
            return message;
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
}
