﻿using System.ComponentModel.DataAnnotations;

namespace AuthService.Handler
{
    public class User
    {
        public int id { get; set; }
        [Required] public string name { get; set; }
        [Required] public string password { get; set; }
        [Required] public string description { get; set; }
    }
}
