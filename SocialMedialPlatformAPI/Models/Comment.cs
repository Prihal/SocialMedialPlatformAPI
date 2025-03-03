﻿
namespace SocialMedialPlatformAPI.Models
{
    public partial class Comment
    {
        public long CommentId { get; set; }

        public long UserId { get; set; }

        public long PostId { get; set; }

        public string? CommentText { get; set; }

        public bool IsDeleted { get; set; }

        public DateTime CreatedDate { get; set; }

        public DateTime? ModifiedDate { get; set; }

        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        public virtual Post Post { get; set; } = null!;

        public virtual User User { get; set; } = null!;
    }

}
