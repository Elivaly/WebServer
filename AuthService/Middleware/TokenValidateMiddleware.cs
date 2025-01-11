using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthService.Middleware
{
    public class TokenValidateMiddleware
    {
        private readonly RequestDelegate _next; public TokenValidateMiddleware(RequestDelegate next) { _next = next; }
        public async Task InvokeAsync(HttpContext context) 
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null) 
            { 
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes("[JWT:Key]");
                try
                { 
                    tokenHandler.ValidateToken(token, new TokenValidationParameters
                    { 
                        ValidateIssuerSigningKey = true, 
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false, 
                        ValidateAudience = false, 
                        ClockSkew = TimeSpan.Zero },
                        out SecurityToken validatedToken); 
                } 
                catch
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");
                    return;
                }
            } 
            await _next(context);
        }
    }
}
