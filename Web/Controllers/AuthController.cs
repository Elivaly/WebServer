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
                _configuration["JWT:Token"]=token;

                HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(3) });

                return Ok(new { token = token});
            }
        }

        [HttpPost]
        [Route("Logout")]
        public IActionResult Loguot() 
        {
            HttpContext.Response.Cookies.Delete("jwtToken");
            _configuration["JWT:Token"] = null;
            return Ok(new { message = "User logged out successfully" }); 
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); 
            var token = new JwtSecurityToken( 
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddMinutes(3),
                signingCredentials: credentials); 
            return new JwtSecurityTokenHandler().WriteToken(token); 
        }
    }
}
