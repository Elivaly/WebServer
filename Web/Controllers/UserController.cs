﻿using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
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
            using (DBC db = new())
            {
                users = db.users
                    .Where(x => x.description.ToLower() == desc.ToLower())
                    .Select(x => x.name)
                    .Distinct()
                    .ToList();
            }
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
            return Ok(user);
        }

        [HttpGet]
        [Route("GetAllUsers")]
        public IActionResult GetAllUsers()
        {
            List<string> users = new List<string>();
            using (DBC db = new())
            {
                users = db.users
                        .Select(x => x.name)
                        .Distinct()
                        .ToList();
            }
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
            using (DBC db = new())
            {
                var user = db.users.FirstOrDefault(x => x.name.ToLower() == name.ToLower());
                if (user == null)
                {
                    return NotFound("No user found with the give name.");
                }
                user.password = newPassword;
                db.SaveChanges();
            }
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
            using (DBC db = new())
            {
                var user = db.users.FirstOrDefault(x => x.name == name);
                if (user == null)
                {
                    return NotFound("No user with the given name.");
                }
                db.users.Remove(user);
                db.SaveChanges();
            }
            return Ok("User was deleted");
        }

        [HttpPost]
        [Route("Test")]
        public IActionResult PostUser([FromQuery][Required] int number)
        {
            if (number <= 0) 
            {
                return BadRequest("Number must be greater than zero");
            }
            return Ok("Success test");
        }
        
        [HttpPost]
        [Route("AddUser")]
        public IActionResult AddUser([FromQuery][Required] string name, [FromQuery][Required] string password, [FromQuery][Required] string desc) 
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(password) || string.IsNullOrEmpty(desc))
            {
                return BadRequest("Data is required.");
            }
            return Ok("User was created");
        }
    }
}
