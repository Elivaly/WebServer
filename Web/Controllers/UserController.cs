using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using AuthService;
using AuthService.Handler;
using AuthService.Schems;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.OpenApi.Extensions;
using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure.Internal;

namespace Web.Controllers
{

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
        [Route("GetByName")]
        public IActionResult GetByName([FromQuery][Required] string name) 
        {
            if (HttpContext == null)
            {
                Console.WriteLine("HttpContext is null");
                return StatusCode(500, "Internal server error: HttpContext is null");
            }
            Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
            Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

            if (string.IsNullOrEmpty(name)) 
            { 
                return BadRequest("Имя отсутствует"); 
            }
            List<string> users = new List<string>();
            using (DBC db = new(_configuration))
            {
                users = db.users
                    .Where(x => x.name.ToLower() == name.ToLower())
                    .Select(x => x.name)
                    .Distinct()
                    .ToList();
            }
            if (users == null || users.Count == 0) 
            {
                return NotFound("Пользователей с заданным именем не существует");
            }
            return Ok( new { Users = users});
        }
        
        [HttpGet]
        [Route("GetNamesByDescription")]
        public IActionResult GetNamesByDescription([FromQuery][Required] string description)
        {
            if (HttpContext == null)
            {
                Console.WriteLine("HttpContext is null");
                return StatusCode(500, "Internal server error: HttpContext is null");
            }
            Console.WriteLine($"Request Path: {HttpContext.Request.Path}");
            Console.WriteLine($"Response Status Code: {HttpContext.Response.StatusCode}");

            if (string.IsNullOrEmpty(description)) 
            { 
                return BadRequest("Отсутствует описание роли пользователя"); 
            }
            List <string> users = new List<string>();
            using (DBC db = new(_configuration))
            {
                users = db.users
                    .Where(x => x.description.ToLower() == description.ToLower())
                    .Select(x => x.name)
                    .Distinct()
                    .ToList();
            }
            if (users == null || users.Count == 0) 
            {
                return NotFound("Пользователей с заданным описанием не существует");
            }
            return Ok(new { Users = users });
        }

        [HttpGet]
        [Route("GetNameByIndex")]
        public IActionResult GetNameByIndex([FromQuery][Required] int index) 
        {
            string user;
            if(index <= 0) 
            {
                return BadRequest("Индекс должен быть больше нуля");
            }
            using (DBC db = new(_configuration))
            {
                user = db.users
                        .Where(x => x.id == index)
                        .Select(x => x.name)
                        .Distinct()
                        .FirstOrDefault()!;
            }
            if (user == null) 
            {
                return NotFound("Пользователей с заданным индексом не существует");
            }
            return Ok( new { Name = user });
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
                return NotFound("Пользователи не были найдены"); 
            }
            return Ok( new { Users =  users });
        }

        [HttpPut]
        [Route("UpdatePasswordByIndex")]
        public IActionResult UpdatePasswordByIndex([FromQuery][Required] int index, [FromQuery][Required] string newPassword) 
        {
            if (index <= 0) 
            {
                return BadRequest("Индекс должен быть больше нуля");
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
                    return NotFound("Пользователей с заданным индексом не существует");
                }
                user.password = newPassword;
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
                return BadRequest("Описание роли пользователя отсутствует");
            }
            if(id < 0) 
            {
                return BadRequest("Индекс должен быть больше нуля");
            }
            using(DBC db = new(_configuration)) 
            {
                var user = db.users.FirstOrDefault(x => x.id == id);
                if(user == null) 
                {
                    return NotFound("Пользователей с заданным индексом не существует");
                }
                user.description = newDescription;
                db.SaveChanges();
            }
            return Ok(new { message =  "Описание роли пользователя было изменено" });
        }

        [HttpDelete]
        [Route("DeleteUserByIndex")]
        public IActionResult DeleteUserByIndex([FromQuery][Required] int index) 
        {
            if (index <= 0) 
            {
                return BadRequest("Индекс должен быть больше нуля");
            }
            using (DBC db = new(_configuration))
            {
                var user = db.users.FirstOrDefault(x => x.id == index);
                if (user == null)
                {
                    return NotFound("Пользоваетелей с заданным индексом не существует");
                }
                db.users.Remove(user);
                db.SaveChanges();
            }
            return Ok( new { message = "Пользователь был удален" });
        }

    }
}
