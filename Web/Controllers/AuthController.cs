﻿using System.ComponentModel.DataAnnotations;
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
                if (existingUser == null) 
                {
                    return Unauthorized("Пользователя с таким именем не существует");
                }
                if (existingUser.password != user.password) 
                { 
                    return Unauthorized("Неправильный пароль"); 
                }
               
                var token = GenerateJwtToken(existingUser);
                _configuration["JWT:Token"]=token;

                HttpContext.Response.Cookies.Append("jwtToken", token, new CookieOptions { HttpOnly = true, Secure = false, SameSite = SameSiteMode.Strict, Expires = DateTimeOffset.UtcNow.AddMinutes(1) });

                return Ok(new { token = token});
            }
        }

        [HttpPost]
        [Route("Logout")]
        public IActionResult Loguot() 
        {
            if (HttpContext == null)
            {
                Console.WriteLine("HttpContext is null");
                return StatusCode(500, "Internal server error: HttpContext is null");
            }
            Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
            Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

            HttpContext.Response.Cookies.Delete("jwtToken");
            _configuration["JWT:Token"] = null;
            return Ok(new { message = "Пользователь вышел из системы" }); 
        }

        private string GenerateJwtToken(User user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256); 
            var claims = new List<Claim>() 
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.name),
                new Claim("role", user.description),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            var token = new JwtSecurityToken( 
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credentials); 
            return new JwtSecurityTokenHandler().WriteToken(token); 
        }
    }
}
