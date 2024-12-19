using AuthService.Filter;

namespace AuthService.Attribute;

public class AuthServiceAttribute : Microsoft.AspNetCore.Mvc.TypeFilterAttribute
{
    public AuthServiceAttribute() : base(typeof(AuthServiceFilter)) { }
}
