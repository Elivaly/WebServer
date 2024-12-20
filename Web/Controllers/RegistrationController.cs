using System.ComponentModel.DataAnnotations;
using AuthService.Handler;
using AuthService.Schems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegistrationController : ControllerBase
    {
        [HttpPost]
        [Route("Registration")]
        public IActionResult Registration([FromBody][Required] User user) 
        {
            if (user.id <= 0)
            {
                return BadRequest("Id must be greater than zero");
            }
            using (var db = new DBC())
            {
                var existingUser = db.users.FirstOrDefault(u => u.id == user.id);
                if (existingUser != null)
                {
                    return Conflict("User with such ID exists");
                }
                db.users.Add(user);
                db.SaveChanges();
            }

            return Ok( new { message = "User registrated successfully"});
        }
      
    }
}
