using System.ComponentModel.DataAnnotations;

namespace AuthService.Schems;

public class User
{
    [Key] public int id { get; set; }
    public string name { get; set; }
    public string password { get; set; }
    public string description { get; set; }

}
