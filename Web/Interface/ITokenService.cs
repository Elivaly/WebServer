using System.Security.Claims;

namespace AuthService.Interface
{
    public interface ITokenService
    {
        public string GetAccessToken(IEnumerable<Claim> claims, out DateTime expires);
        public string GetRefreshToken();
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
    }
}
