using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using AuthService.Database;
using AuthService.Handler;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthorizationHandler authorizationHandler) : ControllerBase
    {

        private readonly IAuthorizationHandler _authorizationHandler = authorizationHandler;
        /*
        [HttpPost]
        [Route("Submit")]
        public async Task<IActionResult> Submit([FromBody][Required] string password)
        {
            if (string.IsNullOrEmpty(password)) 
            {
                return BadRequest();
            }
            string ip = HttpContext.Request.Headers["X-Forward-For"];
            string appVersion = AppVersionService.getVersion();

            return Ok(await _authorizationHandler.Submit());
        }*/

        [HttpPost]
        [Route("GenerateJWT")]
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
