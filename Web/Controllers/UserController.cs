using System.ComponentModel.DataAnnotations;
using System.Linq;
using AuthService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Web.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        [HttpGet]
        [Route("GetByName")]
        public async Task<IActionResult> GetByName([FromQuery][Required] string name) 
        {
            if (string.IsNullOrEmpty(name)) 
            { 
                return BadRequest("Name is required."); 
            }
            List<string> users = new List<string>();
            await using (DBC dBC = new DBC())
            {
                users = await dBC.customers
                    .Where(x => x.name == name)
                    .Select(x => x.name)
                    .Distinct()
                    .ToListAsync();
            };
            return Ok(users);
        }

        [HttpGet]
        [Route("GetByDesc")]
        public async Task<IActionResult> GetByDesc([FromQuery][Required] string desc)
        {
            if (string.IsNullOrEmpty(desc)) 
            { 
                return BadRequest("Description is required."); 
            }
            List <string> users = new List<string>();
            await using (DBC dBC = new DBC())
            {
                users = await dBC.customers
                    .Where(x => x.description == desc)
                    .Select(x => x.name)
                    .Distinct()
                    .ToListAsync();
            };
            return Ok(users);
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public async Task<IActionResult> GetAllUsers()
        {
            List<string> users = new List<string>();
            await using (DBC dBC = new()) 
            { 
                users = await dBC.customers
                    .Select(x => x.name)
                    .Distinct()
                    .ToListAsync(); 
            };
            return Ok(users);
        }
    }
}
