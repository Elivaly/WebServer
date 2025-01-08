using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using AuthService.Handler;
using AuthService.Schems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
using XSystem.Security.Cryptography;

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

    [HttpPost]
    [Route("Registration")]
    public IActionResult Registration([FromBody][Required] User user)
    {
        // Проверка доступности HttpContext
        if (HttpContext == null)
        {
            Console.WriteLine("HttpContext is null");
            return StatusCode(500, "Internal server error: HttpContext is null");
        }
        Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
        Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

        using (var db = new DBC(_configuration))
        {
            var name = user.name;
            var password = Hash(user.password);
            user.password = password;
            var role = user.description;
            Console.WriteLine(password);
            #region ValidateChekers
            if (string.IsNullOrEmpty(user.name) || string.IsNullOrEmpty(user.description) || string.IsNullOrEmpty(user.password))
            {
                return Unauthorized(new { message = "Пустая строка" });
            }

            if (SpaceCheck(name) || SpaceCheck(password) || SpaceCheck(role)) 
            {
                return Unauthorized(new { message = "В одной из строк содержатся пробелы" });
            }

            if (SpecialSymbolCheck(name) || SpecialSymbolCheck(role)) 
            {
                return Unauthorized(new {message = "В одной из строк содержатся специальные символы"});
            }

            if (DashCheck(name) || DashCheck(role)) 
            {
                return Unauthorized("В одной из строк содержится тире");
            }
            if (user.name.Length > 50)
            {
                return Unauthorized(new { message = "Длина имени пользователя должна составлять не более 50 символов" });
            }
            if(user.description.Length > 20) 
            {
                return Unauthorized(new { message = "Длина роли пользователя должна составлять не более 20 символов" });
            }
            #endregion

            db.users.Add(user);
            db.SaveChanges();
        };

        var token = GenerateJwtToken(user);
        Response.Headers.Add("Authorization", $"Bearer {token}");
        _configuration["JWT:Token"] = token;
        HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });
        return Ok(new { token });

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
            new Claim("role", user.description),
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
        if(str.Contains("-")) checker = true;
        return checker;
    }

    private string Hash (string password) 
    {
        byte [] data = Encoding.Default.GetBytes(password);
        SHA1 sha = new SHA1CryptoServiceProvider();
        byte[] result = sha.ComputeHash(data);
        password = Convert.ToBase64String(result);
        return password;
    }
}
