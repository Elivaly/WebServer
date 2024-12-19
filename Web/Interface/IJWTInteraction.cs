using AuthService.Object;

namespace AuthService.Interface;

public interface IJWTInteraction
{
    public string GenerateToken(ObjectJWT objectForPayloadOfJWT);

    public ObjectJWT GetDataFromToken(string token);
}
