
using SocialMedialPlatformAPI.DTO;

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
        public void ChangeStoryDuration();
    }
}
