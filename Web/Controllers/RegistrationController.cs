using System.ComponentModel.DataAnnotations;
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
public class RegistrationController : ControllerBase
{
    IConfiguration _configuration;

    public RegistrationController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Регистрация нового пользователя
    /// </summary>
    /// <remarks>
    /// У каждого пользователя должно быть уникальное имя
    /// </remarks>
    /// <response code="400">Некорректно введенные данные</response>
    /// <response code="401">Пользователь с таким логином существует</response>
    /// <response code="500">Во время исполнения произошла внутрисерверная ошибка</response>

    [HttpPost]
    [Route("[action]")]
    public IActionResult SignUp([FromBody][Required] User user)
    {
        using (var db = new DBC(_configuration))
        {
            var password = Hash(user.Password);
            user.Password = password;
            user.Date_Create = DateOnly.FromDateTime(DateTime.Now);
            user.ID_Role = 2;
            #region ValidateChekers
            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                return BadRequest(new { message = "Пустая строка", statusCode = 400 });
            }

            if (SpaceCheck(user.Username) || SpaceCheck(user.Password))
            {
                return BadRequest(new { message = "В одной из строк содержатся пробелы", statusCode = 400 });
            }

            if (SpecialSymbolCheck(user.Username))
            {
                return BadRequest(new { message = "В имени пользователя содержатся специальные символы", statusCode = 400 });
            }

            if (DashCheck(user.Username))
            {
                return BadRequest(new { message = "В имени пользователя содержится тире", statusCode = 400 });
            }
            #endregion

            var existingUser = db.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
            {
                return Unauthorized(new { message = "Пользователь с таким логином уже существует", statusCode = 401 });
            }

            var token = GenerateJwtToken(user);
            HttpContext.Response.Headers.Append("Authorization", $"{token}");


            HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });

            db.Users.Add(user);
            db.SaveChanges();
            Token userToken = new Token
            {
                User_Token = token,
                ID_User = user.ID,
                Expire_Time = TimeOnly.FromDateTime(DateTime.Now.AddMinutes(1))
            };
            db.Tokens.Add(userToken);
            db.SaveChanges();
            return Ok(new { token = token, StatusCode = 200 });
        }
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
    private string Hash(string password)
    {
        byte[] data = Encoding.Default.GetBytes(password);
        SHA1 sha = new SHA1CryptoServiceProvider();
        byte[] result = sha.ComputeHash(data);
        password = Convert.ToBase64String(result);
        return password;
    }
}
