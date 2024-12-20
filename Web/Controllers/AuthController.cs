﻿using System.ComponentModel.DataAnnotations;
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
            using(DBC db = new ()) 
            { 
                var existingUser = db.users.FirstOrDefault(u => u.name == user.name);
                if (existingUser == null || existingUser.password != user.password) 
                { 
                    return Unauthorized("Invalid username or password"); 
                }

                // Добавление токена в cookies
               
                var token = GenerateJwtToken();

                HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = true, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(30) });

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

    }
}
