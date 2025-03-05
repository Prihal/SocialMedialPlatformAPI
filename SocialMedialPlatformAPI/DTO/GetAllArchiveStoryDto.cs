namespace SocialMedialPlatformAPI.DTO
{
    public class GetAllArchiveStoryDto
    {
        public long UserId { get; set; }
        public List<GetArchiveStorys> Stories { get; set; }
    }

    public class GetArchiveStorys
    {
        public long UserId { get; set; }
        public string StoryUrl { get; set; } = null;
        public string StoryName { get; set; } = null;
        public  DateTime CreateDate { get; set; }
    
    }
}
