using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Schems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace AuthService.Controllers
{
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
            var token = HttpContext.Request.Cookies["jwtToken"];
            if (token == null) 
            {
                return BadRequest("Token is missing");
            }

            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token); 
            var username = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
            var role = jwtToken.Claims.FirstOrDefault(c => c.Type == "role")?.Value; 
            var expiration = jwtToken.ValidTo; 
            return Ok(new { Name = username, Role = role, Exp = expiration});
        }

        [HttpGet]
        [Route("CheckTokenTime")]
        public IActionResult CheckTokenTime()
        {
            var token = HttpContext.Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return BadRequest("Token is missing.");
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            var expiration = jwtToken.ValidTo;
            var timeRemaining = expiration - DateTime.UtcNow;
            return Ok(new { expiration, timeRemaining });
        }

        [HttpPost]
        [Route("RefreshTokenTime")]
        public IActionResult RefreshTokenTime([FromBody][Required] User user)
        {
            var token = HttpContext.Request.Cookies["jwtToken"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing");
            }
            var newToken = GenerateJwtToken(user);
            HttpContext.Response.Cookies.Append("jwtToken", newToken, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(30) });
            return Ok(new { message = "Token was refreshed succcessfully" });
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
        /*
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
        */
        
    }
}
