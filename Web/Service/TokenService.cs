using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AuthService.Interface;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.Service;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    public TokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GetAccessToken(IEnumerable<Claim> claims, out DateTime expires)
    {
        expires = DateTime.Now.AddMinutes(1);
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
        JwtSecurityToken token = new(
          issuer: _configuration["JWT:Issuer"],
          audience: _configuration["JWT:Audience"],
          claims: claims,
          expires: expires,
          signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
        );
        JwtSecurityTokenHandler handler = new();
        return handler.WriteToken(token);
    }
    public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
    {
        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
        TokenValidationParameters validationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _configuration["JWT:Issuer"],
            ValidAudience = _configuration["JWT:Audience"],
            IssuerSigningKey = key,
        };
        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);
        var jwtSecurityToken = securityToken as JwtSecurityToken;
        if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            throw new SecurityTokenException("Invalid token");
        return principal;
    }
    public string GetRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
public static class ServiceProviderExtensions
{
    public static void AddTokenService(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<ITokenService>(t => new TokenService(configuration));
    }
}
