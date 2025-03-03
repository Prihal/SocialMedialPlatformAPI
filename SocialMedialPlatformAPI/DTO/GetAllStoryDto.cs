namespace SocialMedialPlatformAPI.DTO;

public class GetAllStoryDto
{
    public long UserId { get; set; }
    public List<GetStorys> Stories { get; set; }
}

public class GetStorys
{
    public long UserId { get; set; }
    public string StoryUrl { get; set; } = null;
    public string StoryName { get; set; } = null;
    public int StoryDuration { get; set; }
}

