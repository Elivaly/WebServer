using System.IdentityModel.Tokens.Jwt;

namespace AuthService.Handler
{
    public class SecretTokenDescriptor 
    {
        public string Secret { get; set; }
        public string Token { get; set; }
        public SecretTokenDescriptor()
        {
            var token = Token;
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token) as JwtSecurityToken;
            if (jsonToken != null)
            {
                var claims = jsonToken.Claims.Select(c => new { c.Type, c.Value }).ToList();
                foreach (var claim in claims)
                {
                    Console.WriteLine($"{claim.Type}: {claim.Value}");
                }
            }
            else
            {
                Console.WriteLine("Invalid token");
            }
        }
    }
}
