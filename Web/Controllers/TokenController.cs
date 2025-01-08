using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using AuthService.Schems;
using IdentityModel.Client;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

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

    [HttpGet]
    [Route("DecodeToken")]
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
        if (token == null) 
        {
            return BadRequest("Токен отсутствует");
        }            
        var defaultToken = _configuration["JWT:Token"];
        if (defaultToken == null) 
        {
            return BadRequest("Токен отсутствует");
        }
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(defaultToken); 
        var expiration = jwtToken.ValidTo;
        var audience = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Aud)?.Value;
        var issuer = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Iss)?.Value;
        var username = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value; 
        var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value;
        return Ok(new { Exp = expiration, Audience = audience, Issuer = issuer, Username = username, Role = role});
    }

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

        var token = HttpContext.Request.Cookies["jwtToken"];
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest("Токен отсутствует");
        }
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var expiration = jwtToken.ValidTo;
        var timeRemaining = expiration - DateTime.UtcNow;
        return Ok(new { expiration, timeRemaining });
    }

    [HttpPost]
    [Route("RefreshTokenTime")]
    public IActionResult RefreshTokenTime()
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
            return Unauthorized("Токен отсутствует"); 
        }
        var data = GetDataFromExpiredToken(token);
        if (data == null)
        {
            return Unauthorized("Данные не обнаружены");
        }
        var newToken = GenerateJwtToken(data);

        _configuration["JWT:Token"] = newToken;

        HttpContext.Response.Cookies.Append("jwtToken", newToken, new CookieOptions{ HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1)});

        Response.Headers.Add("Authorization", $"Bearer {newToken}");

        return Ok(new { token = newToken }); 
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
            new Claim(JwtRegisteredClaimNames.Sub, data.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "unknown"),
            new Claim("role", data.FindFirst(ClaimTypes.Role)?.Value ?? "unknown"),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()) 
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["JWT:Issuer"],
            audience: _configuration["JWT:Audience"], 
            expires: DateTime.Now.AddMinutes(1), 
            claims: claims, 
            signingCredentials: credentials
            ); 
        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString);
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

}
