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

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
            
        }
        

        [HttpPost]
        [Route("Login")] 
        public IActionResult Login([FromBody] User user) 
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
                var existingUser = db.users.FirstOrDefault(u => u.name == user.name);
                if (existingUser == null || existingUser.password != user.password) 
                { 
                    return Unauthorized("Invalid username or password"); 
                }
               
                var token = GenerateJwtToken(existingUser);

                HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(30) });

                //return Ok(new { token });
                return Ok(new { message = "Hello user!"});
            }
        }
        [HttpPost]
        [Route("Logout")]
        public IActionResult Loguot() 
        {
            HttpContext.Response.Cookies.Delete("jwtToken"); 
            return Ok(new { message = "User logged out successfully" }); 
        }

        [HttpPost]
        [Route("RefreshTokenTime")]
        public IActionResult RefreshTokenTime() 
        {
            var token = HttpContext.Request.Cookies["jwtToken"]; 
            if (string.IsNullOrEmpty(token))
            { 
                return Unauthorized("Token is missing");
            }
            var data = GetDataFromExpiredToken(token);
            if (data == null) 
            {
                return Unauthorized("Invalid token"); 
            }
            var newToken = GenerateJwtToken(data);
            HttpContext.Response.Cookies.Append("jwtToken", newToken, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(30) });
            return Ok(new { token = newToken });
        }
        private string GenerateJwtToken(ClaimsPrincipal data)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); 
            var claims = new[] 
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.name),
                new Claim("role", user.description), 
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken( 
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials); 
            return new JwtSecurityTokenHandler().WriteToken(token); 
        }

        private ClaimsPrincipal GetDataFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true, 
                ValidIssuer = _configuration["JWT:Issuer"],
                ValidAudience = _configuration["JWT:Audience"], 
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"])) 
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
}
