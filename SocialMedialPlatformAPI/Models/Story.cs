namespace SocialMedialPlatformAPI.Models;

public partial class Story
{
    public long StoryId { get; set; }

    public long UserId { get; set; }

    public int StoryTypeId { get; set; }

    public string StoryUrl { get; set; } = null!;

    public string StoryName { get; set; } = null!;

    public int? StoryDuration { get; set; }

    public string? Caption { get; set; }

    public bool IsHighlighted { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual MediaType StoryType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
