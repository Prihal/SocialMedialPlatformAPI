namespace SocialMedialPlatformAPI.Models;

public partial class Chat
{
    public long ChatId { get; set; }

    public long FromUserId { get; set; }

    public long ToUserId { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public virtual User FromUser { get; set; } = null!;

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual User ToUser { get; set; } = null!;
}
