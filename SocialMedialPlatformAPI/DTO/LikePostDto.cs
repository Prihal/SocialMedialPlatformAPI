

namespace SocialMedialPlatformAPI.DTO;

public class LikePostDto
{
    public long UserId { get; set; }
    public long PostId { get; set; }
    public bool IsLike { get; set; }
}
