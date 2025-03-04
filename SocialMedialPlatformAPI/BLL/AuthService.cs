using Microsoft.EntityFrameworkCore;
using System.Globalization;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Helpers;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Models;
using SocialMedialPlatformAPI.Utils;
using SocialMedialPlatformAPI.Common;

namespace SocialMedialPlatformAPI.BLL
{
    public class AuthService:IAuthService
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IJwtService _jwtService;
        private readonly Helper _helper;

        public AuthService(AppDbContext context,IConfiguration configuration,IJwtService jwtService,Helper helper) 
        {
          
            _context = context;
            _configuration = configuration;
            _jwtService = jwtService;
            _helper = helper;
        }

        public async Task<bool> ForgotPassword(ForgetPasswordDto forgetPasswordDto)
        {
            try
            {
                User? user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == forgetPasswordDto.UserId && m.IsDeleted != true);
                if (user != null)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(forgetPasswordDto.Password);
                    user.ModifiedDate = DateTime.Now;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    return true;
                }
                return false;
            }
            catch { return false; }
        }

        public async Task<User> GetUser(ForgetPasswordDto forgetPasswordDto)
        {
          
                User? user = await _context.Users.FirstOrDefaultAsync(m => (m.Email == forgetPasswordDto.EmailOrNumberOrUserName && !string.IsNullOrWhiteSpace(m.Email)
                                                                     || m.ContactNumber == forgetPasswordDto.EmailOrNumberOrUserName && !string.IsNullOrWhiteSpace(m.ContactNumber)
                                                                     || m.UserName == forgetPasswordDto.EmailOrNumberOrUserName && !string.IsNullOrWhiteSpace(m.UserName))
                                                                     && m.IsDeleted != true);
                if (user != null)
                {
                    return user;
                }
                throw new ValidationException(CustomErrorMessage.ExistUser, CustomErrorCode.IsNotExist, new List<ValidationError>
                {
                    new ValidationError
                    {
                        message = CustomErrorMessage.ExistUser,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.IsNotExist
                    }
                });   
        }

        public async Task<UserDto> InsertUserData(UserDto userDto)
        {
            User user = _context.Users.FirstOrDefault(op => op.UserId == userDto.UserId && op.IsDeleted != true)?? new();

            user.Email = userDto.Email ?? string.Empty;
            user.ContactNumber = userDto.ContactNumber ?? string.Empty;
            user.Name = userDto.Name ?? string.Empty;
            user.UserName = userDto.UserName ?? string.Empty;
            user.CreatedDate = DateTime.Now;
            user.ModifiedDate= DateTime.Now;
            user.Bio = userDto.Bio;
            user.Link = userDto.Link;
            user.Gender = userDto.Gender;

            if (user.UserId > 0)
            {
                
                user.DateOfBirth = DateTime.TryParseExact(userDto.DateOfBirth, "dd-MM-yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dob) ? dob : DateTime.MinValue;
                user.ModifiedDate = DateTime.Now;
                user.IsPrivate = userDto.IsPrivate;
                _context.Users.Update(user);
            }
            else
            {
                user.Bio = "";
                user.Link = "";
                user.Gender = "";
                user.CreatedDate = DateTime.Now;
                user.ModifiedDate = DateTime.Now;
                user.Password = BCrypt.Net.BCrypt.HashPassword(userDto.Password);
                await _context.Users.AddAsync(user);
            }

            await _context.SaveChangesAsync();
            user.Password = "";

            UserDto userDTO = _helper.UserMapper(user);
            return userDTO;
        }

        public async Task<LoginResponseDto> LoginUser(LoginRequestDto loginRequestDto)
        {
            try
            {
                User? user = await _context.Users.FirstOrDefaultAsync(m =>
                                       ((m.UserName ?? string.Empty).ToLower() == (loginRequestDto.UserID ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.UserName)
                                       || (m.Email ?? string.Empty).ToLower() == (loginRequestDto.UserID ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.Email)
                                       || m.ContactNumber == loginRequestDto.UserID && !string.IsNullOrWhiteSpace(m.ContactNumber))
                                       && m.IsDeleted != true);

                if (!string.IsNullOrWhiteSpace(user.UserName) && user.UserName.ToLower() == loginRequestDto.UserID.ToLower())
                {
                    user.LoginType = "UserName"; 
                }
                else if (!string.IsNullOrWhiteSpace(user.Email) && user.Email.ToLower() == loginRequestDto.UserID.ToLower())
                {
                    user.LoginType = "Email"; 
                }
                else if (!string.IsNullOrWhiteSpace(user.ContactNumber) && user.ContactNumber == loginRequestDto.UserID)
                {
                    user.LoginType = "Phone"; 
                }

                if (user == null)
                {
                    return new LoginResponseDto
                    {
                        Token = "",
                    };
                }
                else
                {
                    if (!BCrypt.Net.BCrypt.Verify(loginRequestDto.Password, user.Password))
                    {
                        return new LoginResponseDto
                        {
                            Token = "",
                        };
                    }
                }
                await _context.SaveChangesAsync();
                user.Password = "";
                LoginResponseDto loginResponceDto = new()
                {
                    Token = _jwtService.GetJWTToken(user),
                };
                
                return loginResponceDto;
            }
            catch
            {
                throw new Exception(CustomErrorMessage.LoginError);
            }
        }

        public async Task<bool> ResetPassword(ResetPasswordRequestDto resetPasswordRequestDto)
        {
            try
            {
                User? user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == resetPasswordRequestDto.UserId && m.IsDeleted != true);
                if (user != null)
                {
                    user.Password = BCrypt.Net.BCrypt.HashPassword(resetPasswordRequestDto.Password);
                    user.ModifiedDate = DateTime.Now;

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    return true;
                }
                return false;
            }
            catch { return false; }
        }
    }
}
