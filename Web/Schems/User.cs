using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Schems;

public class User
{
    [Key]
    [SwaggerSchema(ReadOnly = true)]
    public int ID { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;

    [SwaggerIgnore]
    public DateOnly Date_Create { get; set; }

    [SwaggerIgnore]
    public int ID_Role { get; set; }

}
