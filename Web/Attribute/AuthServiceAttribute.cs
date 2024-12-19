public class AuthServiceAttribute : Microsoft.AspNetCore.Mvc.TypeFilterAttribute
{
    public AuthServiceAttribute() : base(typeof(AuthServiceFilter)) { }
}
