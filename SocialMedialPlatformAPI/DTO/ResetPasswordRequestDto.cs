namespace SocialMedialPlatformAPI.DTO;

public class ResetPasswordRequestDto
{
    public long UserId { get; set; }
    public string? OldPassword { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
}
