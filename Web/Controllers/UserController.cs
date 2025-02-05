using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using AuthService.Handler;
using AuthService.Schemas;
using Microsoft.AspNetCore.Mvc;

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
            }
            else
            {
                int idRole = user2.ID_Role;
                var roleName = db.Roles.Where(u => u.ID_Role == idRole).Select(u => u.Name_Role).FirstOrDefault();
                role = roleName;
            }
            _configuration["UserSettings:Role"] = role;
        }
        return Ok(new { role = role });
    }

    [HttpGet]
    [Route("[action]")]
    public IActionResult GetCurrentRole()
    {
        return Ok(new { role = _configuration["UserSettings:Role"] });
    }

}
