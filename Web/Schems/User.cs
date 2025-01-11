using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Schems;

public class User
{
    [Key]
    [SwaggerSchema(ReadOnly = true)]
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }

    [SwaggerIgnore]
    public DateOnly DateCreate { get; set; }
    
    [SwaggerIgnore]
    public string RefreshToken { get; set; } = string.Empty;

    [SwaggerIgnore]
    public DateTime ExpiresAccess { get; set; }

    [SwaggerIgnore]
    public DateTime ExpiresRefresh { get; set; }

}
