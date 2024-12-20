using System.ComponentModel.DataAnnotations;

namespace AuthService.Schems;

public class Reqistration
{
    [Required] public string name { get; set; }
    [Required] public string password { get; set; }
    public string description = "Client";
}
