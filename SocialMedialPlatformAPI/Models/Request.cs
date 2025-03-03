namespace SocialMedialPlatformAPI.Models;

public partial class Request
{
    public long RequestId { get; set; }

    public long FromUserId { get; set; }

    public long ToUserId { get; set; }

    public bool? IsCloseFriend { get; set; }

    public bool IsAccepted { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual User ToUser { get; set; } = null!;

    public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

   public virtual User FromUser { get; set; } = null!;
}
