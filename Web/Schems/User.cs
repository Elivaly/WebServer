using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Schems;

public class User
{
    [Key]
    [SwaggerSchema(ReadOnly = true)]
    public int id { get; set; }
    public string name { get; set; }
    public string password { get; set; }

    [SwaggerIgnore]
    public DateOnly datecreate { get; set; }
    
    [SwaggerIgnore]
    public string refreshtoken { get; set; } = string.Empty;

    [SwaggerIgnore]
    public DateTime expiresaccess { get; set; }

    [SwaggerIgnore]
    public DateTime expiresrefresh { get; set; }

}
