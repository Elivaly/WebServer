using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthService.Handler;
using AuthService.Handler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController() : ControllerBase
    {

        [HttpPost][Route("Login")] 
        public IActionResult Login([FromBody] User user) 
        {
            using (var db = new DBC()) 
            { 
                var existingUser = db.users.FirstOrDefault(u => u.name == user.name); 
                if (existingUser == null || !BCrypt.Net.BCrypt.Verify(user.password, existingUser.password)) 
                { 
                    return Unauthorized("Invalid username or password"); 
                }
                var token = GenerateJwtToken(); 
                return Ok(new { token }); 
            } 
        }
        private string GenerateJwtToken() 
        { 
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("verysecretverysecretverysecretkeykeykey"));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken( issuer: "yourIssuer", audience: "yourAudience", expires: DateTime.Now.AddMinutes(30), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        } 

        private bool IsValidUser(User user) 
        { 
            return true;
        }

    }
}
