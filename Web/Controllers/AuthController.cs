using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using AuthService.Handler;
using AuthService.Schems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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
    /// Выдает пользователю токен
    /// </remarks>
    /// <response code="400">Некорректные данные</response>
    /// <response code="401">Неверный логин или пароль</response>
    /// <response code="500">В процессе выполнения произошла внутрисерверная ошибка</response>
    [HttpPost]
    [Route("[action]")]
    public IActionResult SignIn([FromBody] User user)
    {
        #region ValidateChekers
        if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
        {
            return BadRequest(new { message = "Пустая строка", StatusCode = 400 });
        }

        if (SpaceCheck(user.Username) || SpaceCheck(user.Password))
        {
            return BadRequest(new { message = "В одной из строк содержатся пробелы", StatusCode = 400 });
        }

        if (SpecialSymbolCheck(user.Username))
        {
            return BadRequest(new { message = "В имени пользователя содержатся специальные символы", StatusCode = 400 });
        }

        if (DashCheck(user.Username))
        {
            return BadRequest(new { message = "В имени пользователя содержится тире", StatusCode = 400 });
        }
        #endregion

        using (DBC db = new(_configuration))
        {
            user.Password = Hash(user.Password);
            var existingUser = db.Users.FirstOrDefault(u => u.Username == user.Username && u.Password == user.Password);
            if (existingUser == null)
            {
                return Unauthorized(new { message = "Неверный логин или пароль", StatusCode = 401 });
            }
            var token = GenerateJwtToken(existingUser);
            HttpContext.Response.Headers.Append("Authorization", $"{token}");
            HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });

            var userToken = db.Tokens.FirstOrDefault(t => t.ID_User == existingUser.ID);
            if (userToken != null)
            {
                userToken.ID_User = existingUser.ID;
                userToken.User_Token = token;
                userToken.Expire_Time = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(1));
                db.Tokens.Update(userToken);
                db.SaveChanges();
            };
            return Ok(new { token = token, StatusCode = 200 });
        }
    }

    /// <summary>
    /// Выход из аккаунта
    /// </summary>
    /// <remarks>
    /// Обнуляет токен пользователя
    /// </remarks>
    /// <response code="400">Пользователь не вошел в систему</response>
    /// <response code="401">Время жизни токена истекло</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>
    [HttpPost]
    [Route("[action]")]
    public IActionResult SignOut()
    {
        var exit = HttpContext.Request.Headers.ContainsKey("Cookie");
        if (exit)
        {
            using (DBC db = new DBC(_configuration))
            {
                var response = HttpContext.Request.Headers["Cookie"].ToString();
                response = response.Replace("jwtToken=", "").Trim();
                Console.WriteLine(response);
                var token = db.Tokens.FirstOrDefault(t => t.User_Token == response);
                if (token != null)
                {
                    token.Expire_Time = TimeOnly.FromDateTime(DateTime.Now);
                    token.User_Token = "";
                    db.Tokens.Update(token);
                    db.SaveChanges();
                }
            }
        }
        if (HttpContext.Request.Headers.ContainsKey("Authorization"))
        {
            HttpContext.Request.Headers.Remove("Authorization");
            Console.WriteLine("Заголовок был удалён");
        }
        HttpContext.Response.Cookies.Delete("jwtToken");
        return Ok(new { message = "Пользователь вышел из системы", StatusCode = 200 });
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
            new Claim(JwtRegisteredClaimNames.Sub, user.ID.ToString())
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

    private bool SpaceCheck(string str) // проверка на наличие пробела в строке 
    {
        bool checker = false;
        if (str.Contains(" ")) checker = true;
        return checker;
    }
    private bool SpecialSymbolCheck(string str)
    {
        bool checker = false;
        string pattern = @"[!@#$%^&*(),.?\"":{}|<>`~/=_+'№;]";
        Regex regex = new Regex(pattern);
        if (regex.IsMatch(str)) checker = true;
        return checker;
    }
    private bool DashCheck(string str)
    {
        bool checker = false;
        if (str.Contains("-")) checker = true;
        return checker;
    }
}
