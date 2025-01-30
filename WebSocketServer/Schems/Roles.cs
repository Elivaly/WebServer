using System.ComponentModel.DataAnnotations;

namespace WebSocketServer.Schems;

public class Roles
{
    [Key]
    public int ID_Role { get; set; }
    public string Name_Role { get; set; }
}
