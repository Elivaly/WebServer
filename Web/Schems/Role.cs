using System.ComponentModel.DataAnnotations;

namespace AuthService.Schems;

public class Role
{
    [Key]
    public int ID_Role { get; set; }
    public string Name_Role { get; set; }
}
