using System.ComponentModel.DataAnnotations;
using Swashbuckle.AspNetCore.Annotations;

namespace AuthService.Schems;

public class Message
{
    [Key]
    [SwaggerIgnore]
    public int ID_Message { get; set; }

    public string Message_Text { get; set; } = null!;

    [SwaggerIgnore]
    public DateTime Datetime_Create { get; set; }

    [SwaggerIgnore]
    public int ID_User { get; set; }

}
