namespace SocialMedialPlatformAPI.DTO;

public class GetStoryDto
{
    public long UserId {  get; set; }
    public List<GetStory> Stories {  get; set; }
}

public class GetStory
{
    public string StoryUrl { get; set; } = null;
    public string StoryName { get; set; } = null;
    public int StoryDuration { get; set; }
}
