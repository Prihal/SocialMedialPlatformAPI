using System.Diagnostics;
using System.Drawing;
using System;
using System.Globalization;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SocialMedialPlatformAPI.Common;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Models;
using SocialMedialPlatformAPI.Utils;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SocialMedialPlatformAPI.BLL
{
    public class ValidationService : IValidationService
    {
        private static readonly string[] AllowedExtensionsProfilePhoto = { ".jpg", ".jpeg", ".png" };
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png", ".mp4" };
        private readonly AppDbContext _context;
        private List<ValidationError> _errors;

        // Regular expression patterns
        private const string EmailRegex = @"^[\w\-\.]+@([\w-]+\.)+[\w-]{2,4}$";
        private const string MobileRegex = @"^[6-9]{1}[0-9]{9}$";
        private const string PasswordRegex = @"^(?=.*[A-Z])(?=.*\d)(?=.*[a-z])(?=.*\W).{7,15}$";
        private const string UserNameRegex = @"^[a-zA-Z0-9][a-zA-Z0-9_.]{7,17}$";
        private const string LinkRegex = @"^(ftp|http|https):\/\/[^""\s]+(?:\/[^""\s]*)?$";
        public ValidationService(AppDbContext context)
        {
           _context = context;
           _errors = new List<ValidationError>();
        }

        public void ValidateEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.EmailRequired,
                    reference = "email",
                    parameter = "email",
                    errorCode = CustomErrorCode.NullEmail
                });
            }
            else if (!Regex.IsMatch(email, EmailRegex))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidEmailFormat,
                    reference = "email",
                    parameter = "email",
                    errorCode = CustomErrorCode.InvalidEmailFormat
                });
            }
        }
        public List<ValidationError> ValidateDateOfBirth(string dateOfBirth)
        {
            if (string.IsNullOrWhiteSpace(dateOfBirth))
            {
                _errors.Add(new ValidationError()
                {
                    parameter = "DateOfBirth",
                    reference = null,
                    errorCode = CustomErrorCode.NullDateOfBirth,
                    message = CustomErrorMessage.NullDateOfBirth
                });
            }
            else
            {
                string[] dateFormats = {
                                            "yyyy-MM-dd",    // 2024-07-08
                                            "dd-MM-yyyy",    // 08-07-2024
                                            "MM/dd/yyyy",    // 07/08/2024
                                            "yyyy/MM/dd",    // 2024/07/08
                                            "dd MMM yyyy",   // 08 Jul 2024
                                            "dd MMMM yyyy",  // 08 July 2024
                                            "MMM dd, yyyy",  // Jul 08, 2024
                                            "MMMM dd, yyyy"  // July 08, 2024
                                        };
                if (!DateTime.TryParseExact(dateOfBirth, dateFormats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                {
                    _errors.Add(new ValidationError()
                    {
                        parameter = "DateOfBirth",
                        reference = null,
                        errorCode = CustomErrorCode.InvalidDateOfBirthFormat,
                        message = CustomErrorMessage.InvalidDateOfBirthFormat
                    });
                }
                else
                {
                    // Check if the date is after today (future date)
                    if (date > DateTime.Today)
                    {
                        _errors.Add(new ValidationError()
                        {
                            parameter = "DateOfBirth",
                            reference = null,
                            errorCode = CustomErrorCode.FutureDateOfBirth,
                            message = CustomErrorMessage.FutureDateOfBirth
                        });
                    }
                }
            }
            return _errors;
        }
        public void ValidateUserName(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.UsernameRequired,
                    reference = "UserName",
                    parameter = "UserName",
                    errorCode = CustomErrorCode.NullUserName
                });
            }
            else if (!Regex.IsMatch(userName, UserNameRegex))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserNameFormat,
                    reference = "UserName",
                    parameter = "UserName",
                    errorCode = CustomErrorCode.InvalidUserNameFormat
                });
            }
        }
        public void ValidatePassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullPassword
                });
            }
            else if (!Regex.IsMatch(password, PasswordRegex))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPasswordFormat,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.InvalidPasswordFormat
                });
            }
        }
        public void ValidateContactNumber(string contactNumber)
        {
            if (!string.IsNullOrWhiteSpace(contactNumber) && !Regex.IsMatch(contactNumber, MobileRegex))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidMobileNumberFormat,
                    reference = "contactNumber",
                    parameter = "contactNumber",
                    errorCode = CustomErrorCode.InvalidMobileNumberFormat
                });
            }
        }

        public List<ValidationError> ValidateMessageId(long messageId)
        {
            if (messageId == 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullUserId,
                    reference = "MessageId",
                    parameter = "MessageId",
                    errorCode = CustomErrorCode.NullUserId
                });
            }
            else if (messageId < 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "MessageId",
                    parameter = "MessageId",
                    errorCode = CustomErrorCode.InvalidUserId
                });
            }
            if (!_context.Messages.Any(m => m.MessageId == messageId && m.IsDeleted != true))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExistUser,
                    reference = "MessageId",
                    parameter = "MessageId",
                    errorCode = CustomErrorCode.IsNotExist
                });
            }
            return _errors;
        }
        public List<ValidationError> ValidateUserId(long userId)
        {
            if (userId == 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.NullUserId
                });
            }
            else if (userId < 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidUserId
                });
            }
            if (!_context.Users.Any(m => m.UserId == userId && m.IsDeleted != true))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExistUser,
                    reference = "userid",
                    parameter = "userid",
                    errorCode = CustomErrorCode.IsNotExist
                });
            }
            return _errors;
        }
        public bool IsUniqueUserName(string userName, long userId)
        {
            User? user = _context.Users.FirstOrDefault(m => ((m.UserName ?? string.Empty).ToLower() == (userName ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.UserName)) && m.IsDeleted == false && (m.UserId <= 0 || m.UserId != userId));
            if (user == null) return false;

            return true;
        }
        public bool IsUniqueEmail(UserDto model)
        {
            //User? user = _dbcontext.Users.FirstOrDefault(m => ((m.Email ?? string.Empty).ToLower() == (model.Email ?? string.Empty).ToLower() && !string.IsNullOrWhiteSpace(m.Email))
            //                           && m.IsDeleted != true && (m.UserId <= 0 || m.UserId != model.UserId));
            User user = _context.Users.FirstOrDefault(m => m.Email.ToLower() == model.Email.ToLower() && m.IsDeleted == false && (m.UserId <= 0 || m.UserId != model.UserId));
            if (user == null) return false;

            return true;
        }
        public bool IsUniquePhoneNumber(UserDto model)
        {
            User? user = _context.Users.FirstOrDefault(m => (m.ContactNumber == model.ContactNumber && !string.IsNullOrWhiteSpace(m.ContactNumber) && !string.IsNullOrWhiteSpace(model.ContactNumber))
                                       && m.IsDeleted != true && (m.UserId <= 0 || m.UserId != model.UserId));
            if (user == null) return false;

            return true;
        }
        public List<ValidationError> ValidateRegistration(UserDto userDto)
        {
            if (userDto.UserId == 0)
            {
                ValidatePassword(userDto.Password ?? string.Empty);
            }
            ValidateEmail(userDto.Email ?? string.Empty);
            ValidateContactNumber(userDto.ContactNumber ?? string.Empty);
            ValidateUserName(userDto.UserName);
            if (IsUniqueUserName(userDto.UserName, userDto.UserId))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.DuplicateUsername,
                    reference = "username",
                    parameter = userDto.UserName,
                    errorCode = CustomErrorCode.IsUserName
                });
            }
            if (IsUniqueEmail(userDto))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.DuplicateEmail,
                    reference = "Email",
                    parameter = userDto.Email,
                    errorCode = CustomErrorCode.IsEmail
                });

            }
            if (IsUniquePhoneNumber(userDto))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.DuplicateNumber,
                    reference = "mobileNumber",
                    parameter = userDto.ContactNumber,
                    errorCode = CustomErrorCode.IsPhoneNumber
                });
            }
            if (userDto.UserId > 0)
            {
                ValidateDateOfBirth(userDto.DateOfBirth ?? string.Empty);
                ValidateUserId(userDto.UserId);
                if (!string.IsNullOrWhiteSpace(userDto.Link) && !Regex.IsMatch(userDto.Link, LinkRegex, RegexOptions.IgnoreCase))
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidLink,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.InvalidLink
                    });
                }
            }
            return _errors;
        }

        public List<ValidationError> ValidateLogin(LoginRequestDto loginRequestDto)
        {
            if (string.IsNullOrWhiteSpace(loginRequestDto.UserID))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.EmailOrMobileOrUsernameRequired,
                    reference = "Email OR MobileNumber OR UserName",
                    parameter = "Email or MobileNumber or UserName",
                    errorCode = CustomErrorCode.NullEmailOrMobileNumberOrUsername
                });
            }
            else
            {
                if (loginRequestDto.Type == "email" && !Regex.IsMatch(loginRequestDto.UserID, EmailRegex))
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidEmailFormat,
                        reference = "Email",
                        parameter = loginRequestDto.UserID,
                        errorCode = CustomErrorCode.InvalidEmailFormat
                    });
                }
                else if (loginRequestDto.Type == "phone" && !Regex.IsMatch(loginRequestDto.UserID, MobileRegex))
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidMobileNumberFormat,
                        reference = "MobileNumber",
                        parameter = loginRequestDto.UserID,
                        errorCode = CustomErrorCode.InvalidMobileNumberFormat
                    });
                }
                else if (loginRequestDto.Type == "username" && !Regex.IsMatch(loginRequestDto.UserID, UserNameRegex))
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidUserNameFormat,
                        reference = "username",
                        parameter = "username",
                        errorCode = CustomErrorCode.InvalidUserNameFormat
                    });
                }

                ValidatePassword(loginRequestDto.Password ?? string.Empty);
            }

            return _errors;
        }

        public List<ValidationError> ValidateForgetPassword(ForgetPasswordDto forgetPasswordDto)
        {
            if (string.IsNullOrWhiteSpace(forgetPasswordDto.EmailOrNumberOrUserName))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.EmailOrMobileOrUsernameRequired,
                    reference = "Email OR MobileNumber OR UserName",
                    parameter = "Email or MobileNumber or UserName",
                    errorCode = CustomErrorCode.NullEmailOrMobileNumberOrUsername
                });
            }
            else
            {
                if (forgetPasswordDto.Type == "email" && !Regex.IsMatch(forgetPasswordDto.EmailOrNumberOrUserName, EmailRegex))
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidEmailFormat,
                        reference = "Email",
                        parameter = "email",
                        errorCode = CustomErrorCode.InvalidEmailFormat
                    });
                }
                else if (forgetPasswordDto.Type == "phone" && !Regex.IsMatch(forgetPasswordDto.EmailOrNumberOrUserName, MobileRegex))
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidMobileNumberFormat,
                        reference = "MobileNumber",
                        parameter = "MobileNumber",
                        errorCode = CustomErrorCode.InvalidMobileNumberFormat
                    });
                }
                else if (forgetPasswordDto.Type == "username" && !Regex.IsMatch(forgetPasswordDto.EmailOrNumberOrUserName, UserNameRegex))
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.InvalidUserNameFormat,
                        reference = "UserName",
                        parameter = "UserName",
                        errorCode = CustomErrorCode.InvalidUserNameFormat
                    });
                }
            }
            return _errors;
        }

        public List<ValidationError> ValidateForgetPasswordData(ForgetPasswordDto forgetPasswordDto)
        {
            ValidateUserId(forgetPasswordDto.UserId);
            ValidatePassword(forgetPasswordDto.Password ?? string.Empty);

            if (string.IsNullOrWhiteSpace(forgetPasswordDto.ConfirmPassword))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ConfirmPasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullConfirmPassword
                });
            }
            else if (forgetPasswordDto.ConfirmPassword != forgetPasswordDto.Password)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordMatch,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.PasswordNOTMatch
                });
            }
            return _errors;
        }

        public List<ValidationError> ValidateResetPassword(ResetPasswordRequestDto resetPasswordRequestDto)
        {
            ValidateUserId(resetPasswordRequestDto.UserId);
            if (string.IsNullOrWhiteSpace(resetPasswordRequestDto.OldPassword))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.OldPasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullOldPassword
                });
            }
            else if (!Regex.IsMatch(resetPasswordRequestDto.OldPassword, PasswordRegex))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPasswordFormat,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.InvalidOldPasswordFormat
                });
            }
            User? user = _context.Users.FirstOrDefault(m => m.UserId == resetPasswordRequestDto.UserId && m.IsDeleted == false);
            if (user != null && !BCrypt.Net.BCrypt.Verify(resetPasswordRequestDto.OldPassword, user.Password))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordNotmatch,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.PasswordNotMatch
                });
            }
            ValidatePassword(resetPasswordRequestDto.Password ?? string.Empty);
            if (string.IsNullOrWhiteSpace(resetPasswordRequestDto.ConfirmPassword))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ConfirmPasswordRequired,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.NullConfirmPassword
                });
            }
            else if (resetPasswordRequestDto.ConfirmPassword != resetPasswordRequestDto.Password)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.PasswordMatch,
                    reference = "Password",
                    parameter = "Password",
                    errorCode = CustomErrorCode.PasswordNOTMatch
                });
            }
            return _errors;
        }

        public List<ValidationError> ValidateProfileFile(IFormFile ProfilePhoto, long userId)
        {
            List<ValidationError> errors = new();

            ValidateUserId(userId);

            if (ProfilePhoto != null)
            {
                string fileExtension = Path.GetExtension(ProfilePhoto.FileName).ToLowerInvariant();
                if (!AllowedExtensionsProfilePhoto.Contains(fileExtension))
                {
                    errors.Add(new ValidationError
                    {
                        message = string.Format(CustomErrorMessage.InvalidPhotoExtension, string.Join(", ", AllowedExtensionsProfilePhoto)),
                        reference = "ProfilePhoto",
                        parameter = "ProfilePhoto",
                        errorCode = CustomErrorCode.InvalidPhotoExtension
                    });
                }

                int maxFileSizeInBytes = 1024 * 1024;
                if (ProfilePhoto.Length > maxFileSizeInBytes)
                {
                    errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.FileSizeLimitExceeded,
                        reference = "ProfilePhoto",
                        parameter = "ProfilePhoto",
                        errorCode = CustomErrorCode.FileSizeLimitExceeded
                    });
                }
            }

            return errors;
        }

        public List<ValidationError> ValidateFollowRequest(FollowRequestDto followRequestDto, long fromuserId)
        {
            if (fromuserId == followRequestDto.ToUserId)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.MatchUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.SameUserId
                });
            }

            if (followRequestDto.ToUserId <= 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "UserId",
                    parameter = "UserId",
                    errorCode = CustomErrorCode.InvalidToUserId
                });
            }
            return _errors;
        }

        public List<ValidationError> ValidateFollowerOrFollowingList(RequestDto<FollowerListRequestDto> requestDto)
        {
            ValidateUserId(requestDto.Model.UserId);
            if(!(requestDto.Model.FollowerOrFollowing=="Follower" || requestDto.Model.FollowerOrFollowing=="Following"))
            {
                _errors.Add(new ValidationError{
                    errorCode=CustomErrorCode.InvalidListType,
                    message=CustomErrorMessage.InvalidType,
                    parameter="Type",
                    reference = "Type",
                });        
            }
            return _errors;
        }
        public List<ValidationError> ValidateRequestId(long requestId)
        {
            if (requestId == 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullRequestId,
                    reference = "requestId",
                    parameter = "requestId",
                    errorCode = CustomErrorCode.NullRequestId
                });
            }
            else if (requestId < 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidRequestId,
                    reference = "requestId",
                    parameter = "requestId",
                    errorCode = CustomErrorCode.InvalidRequestId
                });
            }
            else if (!_context.Requests.Any(m => m.RequestId == requestId && m.IsDeleted != true))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsRequest,
                    reference = "requestId",
                    parameter = "requestId",
                    errorCode = CustomErrorCode.IsNotRequest
                });
            }
            return _errors;
        }
        public List<ValidationError> ValidateRequestAccept(long requestId, string acceptType)
        {
            ValidateRequestId(requestId);
            if (!(acceptType == "Accept" || acceptType == "Cancle"))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidReqType,
                    reference = "reqtype",
                    parameter = "reqtype",
                    errorCode = CustomErrorCode.InvalidReqType
                });
            }
            return _errors;
        }

        public List<ValidationError> ValidateRequestList(RequestDto<FollowRequestDto> requestDto)
        {
            ValidateUserId(requestDto.Model.userId);
            return _errors;
        }


        public List<ValidationError> ValidateCreatePost(CreatePostDto createPostDto)
        {
            if (createPostDto.PostId > 0)
            {
                if (!_context.Posts.Any(m => m.PostId == createPostDto.PostId && m.IsDeleted != true))
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.ExistPost,
                        reference = "postid",
                        parameter = "postid",
                        errorCode = CustomErrorCode.IsNotPost
                    });
                }
            }
            if (createPostDto.PostId <= 0)
            {
                if (createPostDto.File == null || createPostDto.File.Count == 0)
                {
                    _errors.Add(new ValidationError
                    {
                        message = CustomErrorMessage.NullPostPhoto,
                        reference = "Files",
                        parameter = "Files",
                        errorCode = CustomErrorCode.NullProfilePhoto
                    });
                }
                else
                {
                    foreach (var file in createPostDto.File)
                    {
                        string fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                        long fileSizeInBytes = file.Length;

                        if (!AllowedExtensions.Contains(fileExtension))
                        {
                            _errors.Add(new ValidationError
                            {
                                message = string.Format(CustomErrorMessage.InvalidPhotoExtension, string.Join(", ", AllowedExtensionsProfilePhoto)),
                                reference = "Files",
                                parameter = "Files",
                                errorCode = CustomErrorCode.InvalidFileFormat
                            });
                        }
                        if (file.ContentType.Contains("image"))
                        {
                            // Photo file size limit (1 MB)
                            if (fileSizeInBytes > 1 * 1024 * 1024) // 1 MB in bytes
                            {
                                _errors.Add(new ValidationError
                                {
                                    message = CustomErrorMessage.FileSizeLimitExceeded,
                                    reference = "Files",
                                    parameter = "Files",
                                    errorCode = CustomErrorCode.FileSizeLimitExceeded
                                });
                            }
                        }
                        else if (file.ContentType.Contains("video"))
                        {
                            // Video file size limit (3 MB less than)
                            if (fileSizeInBytes > 3 * 1024 * 1024) // 3 MB in bytes
                            {
                                _errors.Add(new ValidationError
                                {
                                    message = CustomErrorMessage.VideoFileSizeLimitExceeded,
                                    reference = "Files",
                                    parameter = "Files",
                                    errorCode = CustomErrorCode.FileSizeLimitExceeded
                                });
                            }
                        }
                    }
                }
            }
            if (!(createPostDto.PostType == "Post" || createPostDto.PostType == "Reel"))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPostType,
                    reference = "post",
                    parameter = "post",
                    errorCode = CustomErrorCode.InvalidPostType
                });
            }
            return _errors;
        }

        public List<ValidationError> ValidateGetPostById(long postId, string postType)
        {
            ValidateGetPostId(postId);
            if (!(postType == "Post" || postType == "Reel"))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPostType,
                    reference = "typepost",
                    parameter = "typepost",
                    errorCode = CustomErrorCode.InvalidPostType
                });
            }
            return _errors;
        }
        public List<ValidationError> ValidateGetPostId(long postId)
        {
            if (postId == 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullPostId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.NullPostId
                });
            }
            else if (postId < 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPostId,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.InvalidPostId
                });
            }
            else if (!_context.Posts.Any(m => m.PostId == postId && m.IsDeleted != true))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExistPost,
                    reference = "postid",
                    parameter = "postid",
                    errorCode = CustomErrorCode.IsNotPost
                });
            }
            return _errors;
        }

        public List<ValidationError> ValidatePostList(RequestDto<PostListRequestDto> postListRequestDto)
        {
            ValidateUserId(postListRequestDto.Model.UserId);
            if (!(postListRequestDto.Model.PostType == "Post" || postListRequestDto.Model.PostType == "Reel"))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidPostType,
                    reference = "typepost",
                    parameter = "typepost",
                    errorCode = CustomErrorCode.InvalidPostType
                });
            }
            return _errors; ;
        }

        public List<ValidationError> ValidateLikePost(long userId, long PostId)
        {
            ValidateUserId(userId);
            ValidateGetPostId(PostId);
            return _errors;
            
        }

        public List<ValidationError> ValidateCommentPost(CommentPostDto commentPostDto)
        {
            ValidateUserId(commentPostDto.UserId);
            ValidateGetPostId(commentPostDto.PostId);
            if (string.IsNullOrWhiteSpace(commentPostDto.CommentText))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.CommentRequired,
                    reference = "comment",
                    parameter = "comment",
                    errorCode = CustomErrorCode.NullComment
                });
            }
            return _errors;

        }

        public List<ValidationError> ValidateCommentId(long commentId)
        {
            if (commentId == 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullCommentId,
                    reference = "commentId",
                    parameter = "commentId",
                    errorCode = CustomErrorCode.NullCommentId
                });
            }
            else if (commentId < 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidCommentId,
                    reference = "commentId",
                    parameter = "commentId",
                    errorCode = CustomErrorCode.InvalidCommentId
                });
            }
            if (!_context.Comments.Any(m => m.CommentId == commentId && m.IsDeleted != true))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExitsPOstComment,
                    reference = "commentId",
                    parameter = "commentId",
                    errorCode = CustomErrorCode.IsNotComment
                });
            }
            return _errors;
        }

        public List<ValidationError> ValidateUserStoryData(long userId, StoryDto storyDto)
        {
            List<ValidationError> errors = new();

            ValidateUserId(userId);

            if (storyDto.File != null)
            {
                string fileExtension = Path.GetExtension(storyDto.File.FileName).ToLowerInvariant();
                if (!AllowedExtensions.Contains(fileExtension))
                {
                    errors.Add(new ValidationError
                    {
                        message = string.Format("Not Allow", string.Join(", ", AllowedExtensions)),
                        reference = "Story",
                        parameter = "Story",
                        errorCode = "INVALID_EXTENSION"
                    });
                }
    
            }

        return errors;
        }

        public List<ValidationError> ValidateStoryId(long storyId)
        {
            if (storyId == 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.NullUserId,
                    reference = "storyId",
                    parameter = "storyId",
                    errorCode = CustomErrorCode.NullUserId
                });
            }
            else if (storyId < 0)
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.InvalidUserId,
                    reference = "storyId",
                    parameter = "storyId",
                    errorCode = CustomErrorCode.InvalidUserId
                });
            }
            if (!_context.Stories.Any(m => m.StoryId== storyId && m.IsDeleted != true))
            {
                _errors.Add(new ValidationError
                {
                    message = CustomErrorMessage.ExistUser,
                    reference = "storyId",
                    parameter = "storyId",
                    errorCode = CustomErrorCode.IsNotExist
                });
            }
            return _errors;
        }
    }
}

