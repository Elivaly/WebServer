using System.Text;
using AuthService.Schems;
using Microsoft.IdentityModel.Tokens;

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
