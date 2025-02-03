using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
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

    /// <summary>
    /// Получение всех существующих пользователей в БД
    /// </summary>
    /// <remarks>
    /// Для удобства проверки регистрации
    /// </remarks>
    /// <response code="404">Пользовательские данные отсутствуют</response>
    [HttpGet]
    [Route("[action]")]
    public IActionResult GetAllUsers()
    {
        List<User> users = new List<User>(); 
        using (DBC db = new(_configuration)) 
        { 
            users = db.Users.OrderBy(x => x.ID).ToList(); 
        }
        if (users == null || users.Count == 0)
        {
            return NotFound(new { message = "Пользовательские данные отсутствуют", StatusCode = 404 }); 
        }
        return Ok( new { Users =  users, StatusCode = 200 });
    }

    /// <summary>
    /// Изменение пароля пользователя по ID
    /// </summary>
    /// <remarks>
    /// Для изменения существующего пароля, на случай если забылся старый.
    /// </remarks>
    /// <response code="400">Некорректные данные</response>
    /// <response code="404">Пользователь не существует</response>
    [HttpPut]
    [Route("[action]")]
    public IActionResult UpdatePasswordById([FromQuery][Required] int index, [FromQuery][Required] string newPassword) 
    {
        if (index <= 0) 
        {
            return BadRequest(new { message = "Индекс меньше нуля", StatusCode = 400 });
        }
        if (string.IsNullOrEmpty(newPassword)) 
        { 
            return BadRequest(new { message = "Обязательное поле для заполнения пропущено", StatusCode = 400 }); 
        }
        using (DBC db = new(_configuration))
        {
            var user = db.Users.FirstOrDefault(x => x.ID == index);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не существует", StatusCode = 404 });
            }
            user.Password = Hash(newPassword);
            db.SaveChanges();
        }

        return Ok( new {message =  "Пароль был изменен", StatusCode = 200 });
    }

    /// <summary>
    /// Удаление пользователя по ID
    /// </summary>
    /// <response code="400">Некорректно введенные данные</response>
    /// <response code="404">Пользователь не существует</response>
    [HttpDelete]
    [Route("[action]")]
    public IActionResult DeleteUserById([FromQuery][Required] int index) 
    {
        if (index <= 0) 
        {
            return BadRequest(new { message = "Индекс меньше нуля", StatusCode = 400 });
        }
        using (DBC db = new(_configuration))
        {
            var user = db.Users.FirstOrDefault(x => x.ID == index);
            if (user == null)
            {
                return NotFound(new { message = "Пользователь не существует", StatusCode = 404 });
            }
            db.Users.Remove(user);
            db.SaveChanges();
        }
        return Ok( new { message = "Пользователь был удален", StatusCode = 200 });
    }

    /// <summary>
    /// Получение роли пользователя
    /// </summary>
    /// <remarks>
    /// Возвращает название роли пользователя на основе имени
    /// </remarks>
    /// <response code="404">Пользователь не существует</response>
    [HttpGet]
    [Route("GetUserRole")]
    public IActionResult GetUserRole([FromQuery][Required] string name) 
    {
        string role = "User";
        using (DBC db = new(_configuration)) 
        {
            var user1 = db.Users.FirstOrDefault(x => x.Username == name);
            var user1ID = user1.ID;
            string decodeName = HttpUtility.UrlDecode(name);
            var user2 = db.Users.FirstOrDefault(x => x.Username == decodeName);
            var user2ID = user2.ID;
            if (user1 == null && user2 == null)
            {
                return NotFound(new { message = "Пользователь не существует", StatusCode = 404 });
            }
            if (user1 != null)
            {
                int idRole = user1.ID_Role;
                var roleName = db.Roles.Where(u => u.ID_Role == idRole).Select(u => u.Name_Role).FirstOrDefault();
                role = roleName;
                _configuration["UserSettings:ID"] = user1ID.ToString();
            }
            else 
            {
                int idRole = user2.ID_Role;
                var roleName = db.Roles.Where(u => u.ID_Role == idRole).Select(u => u.Name_Role).FirstOrDefault();
                role = roleName;
                _configuration["UserSettings:ID"] = user2ID.ToString();
            }
            _configuration["UserSettings:Role"] = role;
        }
        return Ok(new { role = role});
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult GetCurrentRole() 
    {
        return Ok( new { role = _configuration["UserSettings:Role"] });
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult GetCurrentID()
    {
        return Ok(new { id = int.Parse(_configuration["UserSettings:ID"]) });
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
