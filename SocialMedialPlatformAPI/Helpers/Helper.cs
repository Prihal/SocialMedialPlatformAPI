using System.Net.Mail;
using System.Net;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Models;
using Microsoft.EntityFrameworkCore;
using static SocialMedialPlatformAPI.Utils.Enum;
using SocialMedialPlatformAPI.Hubs;

namespace SocialMedialPlatformAPI.Helpers
{
    public class Helper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _context;
        private readonly NotificationHub _notificationHub;

        public Helper(IHttpContextAccessor httpContextAccessor,AppDbContext context,NotificationHub notificationHub)
        {
            _httpContextAccessor = httpContextAccessor;
            _context = context;
            _notificationHub = notificationHub;
        }
        public UserDto UserMapper(User user)
        {
            UserDto userDTO = new()
            {
                UserId = user.UserId,
                UserName = user.UserName ?? "",
                Email = user.Email,
                Name = user.Name,
                Bio = user.Bio,
                Link = user.Link,
                Gender = user.Gender ?? string.Empty,
                DateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                ProfilePictureName = user.ProfilePictureName ?? string.Empty,
                ProfilePictureUrl = user.ProfilePictureUrl ?? string.Empty,
                ContactNumber = user.ContactNumber ?? string.Empty,
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                // Map other properties as needed
            };
            return userDTO;
        }

        public async Task<bool> EmailSender(string email, string subject, string htmlMessage)
        {
            try
            {
                var mail = "gforgenius2024@gmail.com";
                var password = "iljfsjdtdueqyfhk";

                var client = new SmtpClient("smtp.gmail.com", 587)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(mail, password)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(mail),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public long GetUserIdClaim()
        {
            var userIdClaim = _httpContextAccessor?.HttpContext?.User.FindFirst("UserId");

            if (userIdClaim != null && long.TryParse(userIdClaim.Value, out long userId))
            {
                return userId;
            }
            return 0;
        }

        
        public async Task CreateNotification(NotificationDto notificationDto)
        {
            IQueryable<Notification> data = _context.Notifications.Include(m => m.FromUser).Where(m => m.FromUserId == notificationDto.FromUserId && m.ToUserId == notificationDto.ToUserId);

            Notification? obj = notificationDto.NotificationTypeId switch
            {
                //NotificationTypeId.PostId => await  data.FirstOrDefaultAsync(m => m.PostId == model.Id),
                NotificationTypeId.LikeId => await data.FirstOrDefaultAsync(m => m.LikeId == notificationDto.Id),
                NotificationTypeId.CommentId => await data.FirstOrDefaultAsync(m => m.CommentId == notificationDto.Id),
                NotificationTypeId.RequestId => await data.FirstOrDefaultAsync(m => m.RequestId == notificationDto.Id),
                NotificationTypeId.StoryId => await data.FirstOrDefaultAsync(m => m.StoryId == notificationDto.Id),
                _ => throw new ArgumentOutOfRangeException(nameof(notificationDto.NotificationId), "Unknown NotificationId type"),
            };

            Notification notification = obj ?? new Notification();

            notification.FromUserId = notificationDto.FromUserId;
            notification.ToUserId = notificationDto.ToUserId;
            notification.NotificationType = (int)notificationDto.NotificationType;
            notification.CreatedDate = DateTime.Now;
            notification.IsDeleted = notificationDto.IsDeleted;
            notification.PostId = notificationDto.PostId ?? null;
            if (obj == null)
            {
                switch (notificationDto.NotificationTypeId)
                {
                    case NotificationTypeId.PostId:
                        notification.PostId = notificationDto.Id;
                        break;
                    case NotificationTypeId.LikeId:
                        notification.LikeId = notificationDto.Id;
                        break;
                    case NotificationTypeId.CommentId:
                        notification.CommentId = notificationDto.Id;
                        break;
                    case NotificationTypeId.RequestId:
                        notification.RequestId = notificationDto.Id;
                        break;
                    case NotificationTypeId.StoryId:
                        notification.StoryId = notificationDto.Id;
                        break;
                }
                _context.Notifications.Add(notification);
            }
            await _context.SaveChangesAsync();

            NotificationResponseDto notificationResponse = new()
            {
                NotificationId = notification.NotificationId,
                UserId = notification.FromUserId,
                UserName = notification.FromUser?.UserName,
                ProfileName = notification.FromUser?.ProfilePictureName,
                Message = GetMessageForNotification(notification),
                StoryId = notification.NotificationType == (int)NotificationType.StoryLiked ? notification.StoryId ?? 0 : 0,
                PostId = (notification.NotificationType == (int)NotificationType.PostLiked || notification.NotificationType == (int)NotificationType.PostCommented) ? notification.PostId ?? 0 : 0,
                Comment = notification.NotificationType == (int)NotificationType.PostCommented ? _context.Comments.FirstOrDefault(m => m.CommentId == notification.CommentId)?.CommentText : null,
                PhotoName = GetPhotoNameForNotification(notification)
            };

            await _notificationHub.SendNotificationToUser((int)notificationDto.ToUserId, notificationResponse);

        }

        private string GetMessageForNotification(Notification n)
        {
            return n.NotificationType switch
            {
                (int)NotificationType.FollowRequest => "requested to follow you.",
                (int)NotificationType.FollowRequestAccepted => "Started following you",
                (int)NotificationType.FollowRequestDeleted => "has deleted your follow request.",
                (int)NotificationType.PostLiked => "liked your Photo.",
                (int)NotificationType.PostCommented => "commented on your post:",
                (int)NotificationType.StoryLiked => "liked your story.",
                _ => "You have a new notification."
            };
        }

        private string? GetPhotoNameForNotification(Notification n)
        {
            if (n.NotificationType == (int)NotificationType.PostLiked || n.NotificationType == (int)NotificationType.PostCommented)
            {
                return GetPostPhotoName(n.PostId);
            }
            if (n.NotificationType == (int)NotificationType.StoryLiked)
            {
                return GetStoryPhotoName(n.StoryId);
            }
            return null;
        }

        private string? GetPostPhotoName(long? postId)
        {
            return _context.PostMappings.FirstOrDefault(p => p.PostId == postId)?.MediaName;
        }

        private string? GetStoryPhotoName(long? storyId)
        {
            return _context.Stories.FirstOrDefault(s => s.StoryId == storyId)?.StoryName;
        }

    }
}
