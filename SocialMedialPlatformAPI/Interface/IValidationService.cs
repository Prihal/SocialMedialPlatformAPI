using SocialMedialPlatformAPI.Common;
using SocialMedialPlatformAPI.DTO;

namespace SocialMedialPlatformAPI.Interface
{
    public interface IValidationService
    {
        //AuthController
        List<ValidationError> ValidateRegistration(UserDto userDto);
        List<ValidationError> ValidateLogin(LoginRequestDto loginRequestDto);
        List<ValidationError> ValidateForgetPassword(ForgetPasswordDto forgetPasswordDto);
        List<ValidationError> ValidateForgetPasswordData(ForgetPasswordDto forgetPasswordDto);
        List<ValidationError> ValidateResetPassword(ResetPasswordRequestDto resetPasswordRequestDto);

        //UserController
        List<ValidationError> ValidateProfileFile(IFormFile ProfilePhoto, long userId);
        List<ValidationError> ValidateFollowRequest(FollowRequestDto followRequestDto,long fromuserId);
        List<ValidationError> ValidateFollowerOrFollowingList(RequestDto<FollowerListRequestDto> requestDto);
        List<ValidationError> ValidateUserId(long UserId);
        List<ValidationError> ValidateRequestAccept(long requestId,string acceptType);
        List<ValidationError> ValidateRequestList(RequestDto<FollowRequestDto> requestDto);

        //PostController
        List<ValidationError> ValidateCreatePost(CreatePostDto createPostDto);
        List<ValidationError> ValidateGetPostById(long postId,string postType);
        List<ValidationError> ValidateGetPostId(long postId);
        List<ValidationError> ValidatePostList(RequestDto<PostListRequestDto> postListRequestDto);
        List<ValidationError> ValidateLikePost(long userId,long PostId);
        List<ValidationError> ValidateCommentPost(CommentPostDto commentPostDto);
        List<ValidationError> ValidateCommentId(long commentId);

        //ChatController
        List<ValidationError> ValidateMessageId(long messageId);

        //storyController

        List<ValidationError> ValidateUserStoryData(long userId,StoryDto storyDto);
        List<ValidationError> ValidateStoryId(long storyId);

    }
}
