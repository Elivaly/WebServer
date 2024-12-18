using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService;
public class Customers
{
    public int id { get; set; }
    [Required] public string name { get; set; }
    [Required] public string password { get; set; }
    [Required] public string description { get; set; }
}
