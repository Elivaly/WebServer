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
        [HttpGet]
        [Route("GetByName")]
        public IActionResult GetByName([FromQuery][Required] string name) 
        {
            if (string.IsNullOrEmpty(name)) 
            { 
                return BadRequest("Name is required."); 
            }
            List<string> users = new List<string>();
            using (DBC db = new())
            {
                users = db.users
                    .Where(x => x.name.ToLower() == name.ToLower())
                    .Select(x => x.name)
                    .Distinct()
                    .ToList();
            }
            if (users == null || users.Count == 0) 
            {
                return NotFound("No users found with given name");
            }
            return Ok( new { Users = users});
        }
        
        [HttpGet]
        [Route("GetNamesByDescription")]
        public IActionResult GetNamesByDescription([FromQuery][Required] string description)
        {
            if (string.IsNullOrEmpty(description)) 
            { 
                return BadRequest("Description is required."); 
            }
            List <string> users = new List<string>();
            using (DBC db = new())
            {
                users = db.users
                    .Where(x => x.description.ToLower() == description.ToLower())
                    .Select(x => x.name)
                    .Distinct()
                    .ToList();
            }
            if (users == null || users.Count == 0) 
            {
                return NotFound("No users found with given description");
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
                return BadRequest("Index must be greater than zero");
            }
            using (DBC db = new())
            {
                user = db.users
                        .Where(x => x.id == index)
                        .Select(x => x.name)
                        .Distinct()
                        .FirstOrDefault();
            }
            if (user == null) 
            {
                return NotFound("No user with given index");
            }
            return Ok( new { Name = user });
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            List<User> users = new List<User>(); 
            using (DBC db = new()) 
            { 
                users = db.users.OrderBy(x => x.id).ToList(); 
            }
            if (users == null || users.Count == 0)
            {
                return NotFound("No users found"); 
            }
            return Ok( new { Users =  users });
        }

        [HttpPut]
        [Route("UpdatePasswordByIndex")]
        public IActionResult UpdatePasswordByIndex([FromQuery][Required] int index, [FromQuery][Required] string newPassword) 
        {
            if (index <= 0) 
            {
                return BadRequest("Index must be greater than zero.");
            }
            if (string.IsNullOrEmpty(newPassword)) 
            { 
                return BadRequest("New password is required."); 
            }
            using (DBC db = new())
            {
                var user = db.users.FirstOrDefault(x => x.id == index);
                if (user == null)
                {
                    return NotFound("No user found with the given index.");
                }
                user.password = newPassword;
                db.SaveChanges();
            }

            return Ok( new {message =  "Password was changed"});
        }

        [HttpPut]
        [Route("UpdateDescriptionById")]
        public IActionResult UpdateDescriptionById([FromQuery][Required] int id, [FromQuery][Required] string newDescription) 
        {
            if (string.IsNullOrEmpty(newDescription)) 
            {
                return BadRequest("Description is required");
            }
            if(id < 0) 
            {
                return BadRequest("Id should be greater than zero");
            }
            using(DBC db = new()) 
            {
                var user = db.users.FirstOrDefault(x => x.id == id);
                if(user == null) 
                {
                    return NotFound("No user foud with the given id");
                }
                user.description = newDescription;
                db.SaveChanges();
            }
            return Ok(new { message =  "Description was changed" });
        }

        [HttpDelete]
        [Route("DeleteUserByIndex")]
        public IActionResult DeleteUserByIndex([FromQuery][Required] int index) 
        {
            if (index <= 0) 
            {
                return BadRequest("Index must be greater than zero");
            }
            using (DBC db = new())
            {
                var user = db.users.FirstOrDefault(x => x.id == index);
                if (user == null)
                {
                    return NotFound("No user found with the given index.");
                }
                db.users.Remove(user);
                db.SaveChanges();
            }
            return Ok( new { message = "User was deleted" });
        }

    }
}
