using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using AuthService;
using AuthService.Handler;
using AuthService.Schems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.OpenApi.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Web.Controllers;


[Route("api/[controller]")]
[ApiController]

public class UserController : ControllerBase
{
    IConfiguration _configuration;

    public UserController(IConfiguration configuration) 
    {
        _configuration = configuration;
    }

    [HttpGet]
    [Route("GetAllUsers")]
    public IActionResult GetAllUsers()
    {
        List<User> users = new List<User>(); 
        using (DBC db = new(_configuration)) 
        { 
            users = db.users.OrderBy(x => x.id).ToList(); 
        }
        if (users == null || users.Count == 0)
        {
            return NotFound(new { message = "Пользователи не были найдены" }); 
        }
        return Ok( new { Users =  users });
    }

    [HttpPut]
    [Route("UpdatePasswordById")]
    public IActionResult UpdatePasswordById([FromQuery][Required] int index, [FromQuery][Required] string newPassword) 
    {
        if (index <= 0) 
        {
            return BadRequest(new { message = "Индекс должен быть больше нуля" });
        }
        if (string.IsNullOrEmpty(newPassword)) 
        { 
            return BadRequest("Новый пароль отсутствует"); 
        }
        using (DBC db = new(_configuration))
        {
            var user = db.users.FirstOrDefault(x => x.id == index);
            if (user == null)
            {
                return NotFound(new { message = "Пользователей с заданным индексом не существует" });
            }
            user.password = Hash(newPassword);
            db.SaveChanges();
        }

        return Ok( new {message =  "Пароль был изменен"});
    }

    [HttpPut]
    [Route("UpdateDescriptionById")]
    public IActionResult UpdateDescriptionById([FromQuery][Required] int id, [FromQuery][Required] string newDescription) 
    {
        if (string.IsNullOrEmpty(newDescription)) 
        {
            return BadRequest(new { message = "Описание роли пользователя отсутствует" });
        }
        if(id < 0) 
        {
            return BadRequest(new { message = "Индекс должен быть больше нуля" });
        }
        using(DBC db = new(_configuration)) 
        {
            var user = db.users.FirstOrDefault(x => x.id == id);
            if(user == null)
            {
                return NotFound(new { message = "Пользователей с заданным индексом не существует" });
            }
            user.role = newDescription;
            db.SaveChanges();
        }
        return Ok(new { message =  "Описание роли пользователя было изменено" });
    }

    [HttpDelete]
    [Route("DeleteUserById")]
    public IActionResult DeleteUserById([FromQuery][Required] int index) 
    {
        if (index <= 0) 
        {
            return BadRequest(new { message = "Индекс должен быть больше нуля" });
        }
        using (DBC db = new(_configuration))
        {
            var user = db.users.FirstOrDefault(x => x.id == index);
            if (user == null)
            {
                return NotFound(new { message = "Пользоваетелей с заданным индексом не существует" });
            }
            db.users.Remove(user);
            db.SaveChanges();
        }
        return Ok( new { message = "Пользователь был удален" });
    }
    private string Hash(string password)
    {
        byte[] data = Encoding.Default.GetBytes(password);
        SHA1 sha = new SHA1CryptoServiceProvider();
        byte[] result = sha.ComputeHash(data);
        password = Convert.ToBase64String(result);
        return password;
    }

}
