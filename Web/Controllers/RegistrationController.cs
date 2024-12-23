using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Handler;
using AuthService.Schems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Npgsql;

namespace AuthService.Controllers
{
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
            using (var db = new DBC(_configuration))
            {
                var existingUser = db.users.FirstOrDefault(u => u.name == user.name);

                if (existingUser != null)
                {
                    return Conflict("User exists");
                }
                db.users.Add(user);
                db.SaveChanges();
            };

            var token = GenerateJwtToken(user);

            HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(30) });

            return Ok( new { message = "User registrated successfully"});
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
                issuer: "yourIssuer",
                audience: "yourAudience",
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
