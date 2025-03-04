using static SocialMedialPlatformAPI.Utils.Enum;

namespace SocialMedialPlatformAPI.DTO;

public class NotificationDto
{
    public long NotificationId { get; set; }
    public long FromUserId { get; set; }
    public long ToUserId { get; set; }
    public NotificationType? NotificationType { get; set; }
    public long Id { get; set; }
    public NotificationTypeId NotificationTypeId { get; set; }
    public bool IsDeleted { get; set; }
    public long? PostId { get; set; }
    public long? storyId { get; set; }

    public DateTime modifiedDate { get; set; }

}
