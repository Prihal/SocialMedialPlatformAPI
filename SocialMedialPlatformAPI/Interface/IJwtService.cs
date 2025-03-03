using SocialMedialPlatformAPI.Models;

namespace SocialMedialPlatformAPI.Interface
{
    public interface IJwtService
    {

        string GetJWTToken(User user);
        string GenerateJwtToken(User user);
    }
}
