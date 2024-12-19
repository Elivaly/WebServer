using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        [HttpGet]
        [Route("GetName")]
        public IActionResult GetName([FromQuery][Required] string name)
        {
            return Ok(name);
        }

        [HttpGet]
        [Route("GetPassword")]
        public IActionResult GetPassword([FromQuery][Required] string password)
        {
            return Ok(password);
        }

        [HttpPost]
        [Route("Submit")]
        public IActionResult Submit([FromBody][Required] string name, [FromQuery][Required] string password)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password)) 
            {
                return BadRequest();
            }
            return Ok("Hello");
        }
    }
}
