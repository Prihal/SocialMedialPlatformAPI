using SocialMedialPlatformAPI.DTO;


namespace SocialMedialPlatformAPI.Interface
{
    public interface IChatService
    {
        Task<bool> SendMessage(long UserId,SendMessageDto sendMessageDto);
        Task<PaginationResponseModel<ResponseMessageDto>> GetAllMessage(long UserId);
        Task<PaginationResponseModel<ResponseMessageDto>> GetMessageById(long FromUserId);
        Task<bool> RemoveAllMessageById(long FromUserId);
        Task<bool> RemoveAllMessages(long userId);
        Task<bool> RemoveMessagesById(long userId,long messageId);


    }
}
