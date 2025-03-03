using SocialMedialPlatformAPI.DTO;

namespace SocialMedialPlatformAPI.Interface
{
    public interface IUserService
    {
        Task<ProfilePhotoResponseDto> UploadProfilePhoto(IFormFile ProfilePhoto, long userId);
        Task<bool> FollowRequest(FollowRequestDto followRequestDto, long fromuserId);
        Task<PaginationResponseModel<UserDto>> FollowerOrFollowingListById(RequestDto<FollowerListRequestDto> requestDto);
        Task<PaginationResponseModel<UserDto>> GetUserListByUserName(RequestDto<UserIdRequestDto> requestDto);
        Task<UserDto> GetUserData(long userId);
        Task<bool> RequestAcceptOrCancel(long requestId, string acceptType);
        Task<PaginationResponseModel<RequestListResponseDto>> GetRequestListById(RequestDto<FollowRequestDto> requestDto);
        Task<CountResponseDto> GetFollowerFollowingPostCountById(long userId);
        Task<GetProfilePhotoDto> GetUserProfilePhoto(long userId);
        string GetContentType(string fileExtension);
       
    }
}
