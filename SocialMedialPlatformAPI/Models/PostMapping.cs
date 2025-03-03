namespace SocialMedialPlatformAPI.Models;

public partial class PostMapping
{
    public long PostMappingId { get; set; }

    public long PostId { get; set; }

    public int MediaTypeId { get; set; }

    public string MediaUrl { get; set; } = null!;

    public string MediaName { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual MediaType MediaType { get; set; } = null!;

    public virtual Post Post { get; set; } = null!;
}
