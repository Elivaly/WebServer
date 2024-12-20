using AuthService.Object;

namespace AuthService.Interface;

public interface IJWTInteraction
{
    public ObjectJWT GetDataFromToken(string token);
}
