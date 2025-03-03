namespace SocialMedialPlatformAPI.Models;

public partial class Post
{
    public long PostId { get; set; }

    public long UserId { get; set; }

    public string? Caption { get; set; }

    public string? Location { get; set; }

    public int PostTypeId { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsSaved { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

    public virtual ICollection<PostMapping> PostMappings { get; set; } = new List<PostMapping>();

    public virtual MediaType PostType { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
