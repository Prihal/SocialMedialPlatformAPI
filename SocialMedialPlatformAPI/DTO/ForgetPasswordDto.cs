﻿namespace SocialMedialPlatformAPI.DTO;

public class ForgetPasswordDto
{
    public long UserId { get; set; }
    public string? EmailOrNumberOrUserName { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Type { get; set; }
    public string? EncyptUserId { get; set; }
}
