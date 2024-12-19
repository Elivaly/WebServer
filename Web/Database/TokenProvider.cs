using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.JsonWebTokens;
using System.IdentityModel.Tokens.Jwt;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace AuthService.Database;
/*
public sealed class TokenProvider(IConfiguration configuration)
{
    public string Create(User user)
    {
        string secretKey = configuration["Jwt:Secret"]!;
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

        var credentials = new SingingCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecretTokenDescriptor
        {
            Subject = new ClaimsIdentity(
                [
                    new Claim(JwtRegisteredClaimNames.Sub, user.id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Name, user.name),
                    new Claim(JwtRegisteredClaimNames.)
                ]),
        };
        return token;
    }
}
*/