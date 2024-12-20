using AuthService.Exceptions;
using AuthService.Interface;
using AuthService.Object;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace AuthService.Services
{
    public class JWTService(IConfiguration config) : IJWTInteraction
    {
        private readonly IConfiguration _config = config;
        private const string userIdClaimName = "userId";

        public string GenerateToken(ObjectJWT objectJWT)
        {
            var secret = _config["InnerSettings:JWT:Secret"];
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);
            var claims = new Dictionary<string, object>();

            var userId = objectJWT.userId;

            claims.Add(userIdClaimName, userId);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Claims = claims,
                Expires = DateTime.UtcNow.AddHours(double.Parse(_config["InnerSettings:JWT:JwtExpirationInHours"])),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature),
                IssuedAt = DateTime.UtcNow
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public ObjectJWT GetDataFromToken(string token)
        {
            if (string.IsNullOrEmpty(token))
                throw new CustomExceptions.JWTIsEmpty();

            var secret = _config["InnerSettings:JWT:Secret"];
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(secret);

            tokenHandler.ValidateToken(
                token,
                new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    // set clockskew to zero so tokens expire exactly at token expiration time (instead of 5 minutes later)
                    ClockSkew = TimeSpan.FromSeconds(int.Parse(_config["InnerSettings:JWT:JwtDelayDuringVerificationInSeconds"]))
                },
                out SecurityToken validatedToken);
            var jwtToken = (JwtSecurityToken)validatedToken;

            string userId = jwtToken.Claims.First(x => x.Type == userIdClaimName).Value;
            ObjectJWT jwtObj = new()
            {
                userId = userId
            };

            return jwtObj;
        }

    }
}
