namespace SocialMedialPlatformAPI.Models
{
    public partial class Notification
    {
        public long NotificationId { get; set; }

        public long FromUserId { get; set; }

        public long ToUserId { get; set; }

        public int NotificationType { get; set; }

        public long? PostId { get; set; }

        public long? LikeId { get; set; }

        public long? CommentId { get; set; }

        public long? StoryId { get; set; }

        public long? RequestId { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual Comment? Comment { get; set; }

        public virtual User FromUser { get; set; } = null!;

        public virtual Like? Like { get; set; }

        public virtual Post? Post { get; set; }

        public virtual Request? Request { get; set; }

        public virtual Story? Story { get; set; }

        public virtual User ToUser { get; set; } = null!;
    }

}
