using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Models;

namespace SocialMedialPlatformAPI.Interface
{
    public interface IAuthService
    {
        Task<UserDto> InsertUserData(UserDto userDto);
        Task<LoginResponseDto> LoginUser(LoginRequestDto loginRequestDto);

        Task<User> GetUser(ForgetPasswordDto forgetPasswordDto);

        Task<bool> ForgotPassword(ForgetPasswordDto forgetPasswordDto);
        Task<bool> ResetPassword(ResetPasswordRequestDto resetPasswordRequestDto);
    }
}
