namespace SocialMedialPlatformAPI.Models
{
    public partial class Message
    {
        public long MessageId { get; set; }

        public long ChatId { get; set; }

        public long FromUserId { get; set; }

        public long ToUserId { get; set; }

        public string MessageText { get; set; } = null!;

        public bool IsSeen { get; set; }

        public bool IsDelivered { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual Chat Chat { get; set; } = null!;

        public virtual User FromUser { get; set; } = null!;

        public virtual User ToUser { get; set; } = null!;
    }

}
