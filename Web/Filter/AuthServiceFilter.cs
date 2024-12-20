using AuthService.Services;
using Microsoft.AspNetCore.Mvc.Filters;
using static AuthService.Exceptions.CustomExceptions;

namespace AuthService.Filter;
/*
public class AuthServiceFilter(IConfiguration config) : IAuthorizationFilter
{
    private readonly JWTService _jwtservice = jwtservice;
    private readonly IConfiguration _config = config;


    public void OnAuthorization(AuthorizationFilterContext context)
    {
        try
        {
            var jwtData = _redisService.GetValue(context.HttpContext.GetRedisKey());
            context.HttpContext.Request.Headers.TryGetValue(_config["InnerSettings:Language:HeaderName"], out var lang);

            UserDataDto cred = new()
            {
                Id = jwtData.TraderData.IDTrader,
                Name = jwtData.TraderData.IDFirm,
                Password = jwtData.SessionKey,
                Description = jwtData.TraderData.IDRole.GetValueOrDefault()
            };

            context.HttpContext.Items.Add("UserData", cred);
        }
        catch
        {
            throw new JWTNotValid();
        }
    }

}*/
