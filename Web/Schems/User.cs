using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Schems;

public class User
{
    [Key]
    [SwaggerSchema(ReadOnly = true)]
    public int id { get; set; }
    public string name { get; set; }
    public string password { get; set; }
    public string role { get; set; }

}
