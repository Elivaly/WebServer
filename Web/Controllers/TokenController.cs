using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
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
            var token = _configuration["JWT:Token"];
            if (token == null) 
            {
                return BadRequest("Token is missing");
            }
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token); 
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
        public IActionResult RefreshTokenTime()
        {
            var token = _configuration["JWT:Token"];
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Token is missing");
            }
            var data = GetDataFromExpiredToken(_configuration["JWT:Token"]);
            if (data == null)
            {
                return Unauthorized("Invalid token");
            }
            var newToken = GenerateJwtToken(data);
            _configuration["JWT:Token"] = token;
            HttpContext.Response.Cookies.Append("jwtToken", newToken, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(3) });
            return Ok(new { token });
        }
        
        private string GenerateJwtToken(ClaimsPrincipal data)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var claims = data.Claims.ToList();
            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.Now.AddMinutes(3),
                claims: claims,
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
            var claims = data.Claims.ToList();
            for ( var i = 0; i < claims.Count; i++) 
            { 
                Console.WriteLine(claims[i]);
            }
            var jwtToken = securityToken as JwtSecurityToken;
            if (jwtToken == null || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }
            return data;
        }

    }
}
