using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using AuthService.Handler;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AuthService.Handler;

public sealed class TokenProvider(IConfiguration configuration)
{
    private string token;
    public string Create(User user)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var tokenDescriptor = new SecretTokenDescriptor
        {
            /*Subject = new ClaimsIdentity([
                      new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
                      new Claim(JwtRegisteredClaimNames.Name, user.name)]),
        */};
        return token;
    }
}
