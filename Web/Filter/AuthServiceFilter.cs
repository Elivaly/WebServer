using AuthService.Handler;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using static AuthService.Exceptions.CustomExceptions;

namespace AuthService.Filter;

public class AuthServiceFilter(IConfiguration config) : IAuthorizationFilter
{
    private readonly JWTService _jwtservice;
    private readonly IConfiguration _config; 
    private readonly DBC _dbContext; 
    public void OnAuthorization(AuthorizationFilterContext context)
    { 
        try 
        { 
            var token = context.HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", ""); 
            var jwtData = _jwtservice.DecodeToken(token);
            var user = _dbContext.users.FirstOrDefault(u => u.id == jwtData.userData.id);
            if (user == null)
            {
                throw new JWTNotValid(); 
            } 
            context.HttpContext.Request.Headers.TryGetValue(_config["InnerSettings:Language:HeaderName"], out var lang);
            UserDataDto cred = new()
            { 
                Id = user.id, 
                Name = user.name, 
                Password = user.password, 
                Description = user.description 
            }; 
            context.HttpContext.Items.Add("UserData", cred); } catch { throw new JWTNotValid(); } }
}
