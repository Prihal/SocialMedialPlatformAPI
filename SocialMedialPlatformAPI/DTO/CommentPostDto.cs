namespace SocialMedialPlatformAPI.DTO;
public class CommentPostDto
{
    public long UserId {  get; set; }
    public long PostId {  get; set; }
    public string? CommentText { get; set; }
}
