
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Models;

namespace SocialMedialPlatformAPI.Interface
{
    public interface IStoryServicecs
    {
        Task<bool> AddStory(long userId, StoryDto storyDto);
        Task<bool> DeleteStory(long userId,long storyId);
        Task<bool> SetHighlight(long userId,long storyId);
        Task<bool> RemoveHighlight(long userId,long storyId);
        Task<PaginationResponseModel<GetStoryDto>> GetStory(long userId);
        Task<PaginationResponseModel<GetAllStoryDto>> GetAllStory(long userId);
        Task<PaginationResponseModel<GetAllArchiveStoryDto>> GetAllArchiveStory(long userId);
        public void ChangeStoryDuration();
    }
}
