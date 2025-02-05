using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Schems;

public class Token
{
    [Key]
    [SwaggerIgnore]
    public int ID_Token { get; set; }
    public int ID_User { get; set; }
    public string User_Token { get; set; }
    public TimeOnly Expire_Time { get; set; }
}
