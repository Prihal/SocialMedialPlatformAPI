namespace SocialMedialPlatformAPI.Models;

public partial class User
{
    public long UserId { get; set; }

    public string? UserName { get; set; }

    public string Email { get; set; } = null!;

    public string? Password { get; set; }

    public string? Gender { get; set; }

    public string? ContactNumber { get; set; }

    public DateTime? DateOfBirth { get; set; }

    public string? ProfilePictureUrl { get; set; }

    public string? ProfilePictureName { get; set; }

    public string? Link { get; set; }

    public string? Bio { get; set; }

    public string? Name { get; set; }

    public bool IsVerified { get; set; }

    public bool IsPrivate { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? ModifiedDate { get; set; }

    public string? LoginType { get; set; }

    public virtual ICollection<Chat> ChatFromUsers { get; set; } = new List<Chat>();

    public virtual ICollection<Chat> ChatToUsers { get; set; } = new List<Chat>();

    public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();

    public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

    public virtual ICollection<Message> MessageFromUsers { get; set; } = new List<Message>();

    public virtual ICollection<Message> MessageToUsers { get; set; } = new List<Message>();

    public virtual ICollection<Notification> NotificationFromUsers { get; set; } = new List<Notification>();

    public virtual ICollection<Notification> NotificationToUsers { get; set; } = new List<Notification>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

    public virtual ICollection<Request> RequestFromUsers { get; set; } = new List<Request>();

    public virtual ICollection<Request> RequestToUsers { get; set; } = new List<Request>();

    public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
}
