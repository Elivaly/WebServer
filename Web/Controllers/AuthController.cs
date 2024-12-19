using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthService.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {

        [HttpPost]
        [Route("Submit")]
        public IActionResult Submit([FromBody][Required] string name, [FromQuery][Required] string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password)) 
            {
                return BadRequest();
            }
            return Ok("Hello");
        }

        [HttpPost]
        [Route("Generate JWT")]
        public IActionResult Login([FromBody] User user)
        { 
            if (IsValidUser(user)) 
            { 
                var token = GenerateJwtToken();
                return Ok(new { token });
            } 
            return Unauthorized(); 
        } 
        private string GenerateJwtToken() 
        { 
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("yourSecretKey"));
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
