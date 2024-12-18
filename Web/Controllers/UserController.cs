using System.ComponentModel.DataAnnotations;
using System.Linq;
using AuthService;
using AuthService.Database;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.OpenApi.Extensions;

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
            using (DBC dBC = new ())
            {
                users = dBC.users
                    .Where(x => x.name == name)
                    .Select(x => x.name)
                    .Distinct()
                    .ToList();
            };
            if (users == null || users.Count == 0) 
            {
                return NotFound("No users found with given name");
            }
            return Ok(users);
        }

        [HttpGet]
        [Route("GetByDesc")]
        public IActionResult GetByDesc([FromQuery][Required] string desc)
        {
            if (string.IsNullOrEmpty(desc)) 
            { 
                return BadRequest("Description is required."); 
            }
            List <string> users = new List<string>();
            using (DBC dBC = new ())
            {
                users = dBC.users
                    .Where(x => x.description == desc)
                    .Select(x => x.name)
                    .Distinct()
                    .ToList();
            };
            if (users == null || users.Count == 0) 
            {
                return NotFound("No users found with given description");
            }
            return Ok(users);
        }

        [HttpGet]
        [Route("GetByIndex")]
        public IActionResult GetByIndex([FromQuery][Required] int index) 
        {
            string user;
            if(index <= 0) 
            {
                return BadRequest("Index must be greater than zero");
            }
            using (DBC dBC = new ()) 
            {
                user = dBC.users
                    .Where(x => x.id == index)
                    .Select(x => x.name)
                    .First();
            };
            if (user == null) 
            {
                return NotFound("No user with given index");
            }
            return Ok(user);
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            List<string> users = new List<string>();
            using (DBC dBC = new()) 
            { 
                users = dBC.users
                    .Select(x => x.name)
                    .Distinct()
                    .ToList(); 
            };
            if(users == null || users.Count == 0) 
            {
                return NotFound("No users found");
            }
            return Ok(users);
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
            using(DBC dBC = new ()) 
            {
                var user = dBC.users.FirstOrDefault(x => x.id == index);
                if (user == null)
                {
                    return NotFound("No user found with the given index.");
                } 
                user.password = newPassword; 
                dBC.SaveChanges(); 
            };
            return Ok("Password was changed");
        }

        [HttpPut]
        [Route("UpdatePasswordByName")]
        public IActionResult UpdatePasswordByname([FromQuery][Required] string name, [FromQuery][Required] string newPassword) 
        {
            if (string.IsNullOrEmpty(name)) 
            {
                return BadRequest("Name is required.");
            }
            if (string.IsNullOrEmpty(newPassword)) 
            {
                return BadRequest("Password is required.");
            }
            using(DBC dBC = new()) 
            {
                var user = dBC.users.Find(name);
                if(user == null) 
                {
                    return NotFound("No user found with the give name.");
                }
                user.password = newPassword;
                dBC.SaveChanges();
            };
            return Ok("Password was changed");

        }

        [HttpDelete]
        [Route("DeleteUserByIndex")]
        public IActionResult DeleteUserByIndex([FromQuery][Required] int index) 
        {
            if (index <= 0) 
            {
                return BadRequest("Index must be greater than zero");
            }
            using (DBC dBC = new ()) 
            {
                var user = dBC.users.FirstOrDefault(x => x.id == index); 
                if (user == null) 
                { 
                    return NotFound("No user found with the given index."); 
                }
                dBC.users.Remove(user);
                dBC.SaveChanges();
            };
            return Ok("User was deleted");
        }

        [HttpDelete]
        [Route("DeleteUserByName")]
        public IActionResult DeleteUserByName([FromQuery][Required] string name) 
        {
            if (string.IsNullOrEmpty(name)) 
            {
                return BadRequest("Name is required");
            }
            using (DBC dBC = new()) 
            {
                var user =  dBC.users.FirstOrDefault(x => x.name == name);
                if (user == null)
                { 
                    return NotFound("No user with the given name.");
                }
                dBC.users.Remove(user);
                dBC.SaveChanges();
            }
            return Ok("User was deleted");
        }

    }
}
