using SocialMedialPlatformAPI.DTO;

namespace SocialMedialPlatformAPI.Interface
{
    public interface INotificationService
    {
        Task<PaginationResponseModel<NotificationResponseDto>> GetNotificationListById(PaginationRequestDto model);
        Task<bool> DeteleNotificationAsync(List<long> notificationId);
    }
}
