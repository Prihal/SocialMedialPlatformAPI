using Microsoft.EntityFrameworkCore;
using SocialMedialPlatformAPI.Common;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Helpers;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Models;
using SocialMedialPlatformAPI.Utils;
using static SocialMedialPlatformAPI.Utils.Enum;

namespace SocialMedialPlatformAPI.BLL
{
    public class UserService : IUserService
    {
        public readonly AppDbContext _context;
        public readonly Helper _helper;
        public UserService(AppDbContext context, Helper helper)
        {
            _helper = helper;
            _context =context;
        }

        public async Task<PaginationResponseModel<UserDto>> FollowerOrFollowingListById(RequestDto<FollowerListRequestDto> requestDto)
        {
            IQueryable<UserDto> data = requestDto.Model.FollowerOrFollowing
            switch
            {
            "Follower" => _context.Requests.Include(m => m.FromUser).Where(m => m.ToUserId == requestDto.Model.UserId && m.IsAccepted != false && m.IsDeleted != true)
            .Select(m=>new UserDto
                { 
                   UserId=m.FromUser.UserId,
                   UserName=m.FromUser.UserName,
                   Email=m.FromUser.Email,
                   Name=m.FromUser.Name,
                   Bio=m.FromUser.Bio,
                   Link=m.FromUser.Link,
                   Gender=m.FromUser.Gender,
                   ProfilePictureName=m.FromUser.ProfilePictureName,
                   ProfilePictureUrl=m.FromUser.ProfilePictureUrl,
                   ContactNumber=m.FromUser.ContactNumber,
                   IsPrivate=m.FromUser.IsPrivate,
                   IsVerified=m.FromUser.IsVerified,
                }),
             "Following"=> _context.Requests.Include(m => m.ToUser).Where(m => m.FromUserId == requestDto.Model.UserId && m.IsAccepted != false && m.IsDeleted != true)
            .Select(m => new UserDto
            {
                UserId = m.ToUser.UserId,
                UserName = m.ToUser.UserName,
                Email = m.ToUser.Email,
                Name = m.ToUser.Name,
                Bio = m.ToUser.Bio,
                Link = m.ToUser.Link,
                Gender = m.ToUser.Gender,
                ProfilePictureName = m.ToUser.ProfilePictureName,
                ProfilePictureUrl = m.ToUser.ProfilePictureUrl,
                ContactNumber = m.ToUser.ContactNumber,
                IsPrivate = m.ToUser.IsPrivate,
                IsVerified = m.ToUser.IsVerified,
            }),
                _ => throw new ArgumentException("Invalid Follower Or Following Value")
            };  
            int totalRecored=await data.CountAsync();
            int requiredPage = (int)Math.Ceiling((decimal)totalRecored/requestDto.PageSize);

            List<UserDto> recored=await data
                .Skip((requestDto.PageNumber-1)*requestDto.PageSize)
                .Take(requestDto.PageSize)
                .ToListAsync();
            return new PaginationResponseModel<UserDto>
            {
                Records = recored,
                TotalRecord = totalRecored,
                RequiredPage = requiredPage,
                PageSize = requestDto.PageSize,
                PageNumber = requestDto.PageNumber,
            };         
        }

        public async Task<bool> FollowRequest(FollowRequestDto followRequestDto, long fromuserId)
        {
            try
            {
                if (!await _context.Users.AnyAsync(m => m.UserId == followRequestDto.ToUserId) ||
                    !await _context.Users.AnyAsync(m => m.UserId == fromuserId))
                {
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

                Request? data = await _context.Requests.FirstOrDefaultAsync(m => m.FromUserId == fromuserId && m.ToUserId == followRequestDto.ToUserId);
                Request obj = data ?? new();

                obj.FromUserId = fromuserId;
                obj.ToUserId = followRequestDto.ToUserId;
                obj.CreatedDate = DateTime.Now;
                obj.ModifiedDate = DateTime.Now;
                obj.IsCloseFriend = false;

                bool isPrivateUser = _context.Users.FirstOrDefault(m => m.UserId == followRequestDto.ToUserId && m.IsDeleted == false)?.IsPrivate ?? false;

                if (!isPrivateUser)
                {
                    obj.IsAccepted = true;
                }

                if (data == null)
                {
                    
                    await _context.Requests.AddAsync(obj);
                }
                else
                {
                    obj.ModifiedDate = DateTime.Now;

                    obj.IsDeleted = !data.IsDeleted;
                    if (obj.IsDeleted == true)
                    {
                        obj.IsAccepted = false;
                    }
                }
                await _context.SaveChangesAsync();

                if (isPrivateUser)
                {
                    await _helper.CreateNotification(new NotificationDto()
                    {
                        FromUserId = fromuserId,
                        ToUserId = followRequestDto.ToUserId,
                        NotificationType = NotificationType.FollowRequest,
                        NotificationTypeId = NotificationTypeId.RequestId,
                        Id = obj.RequestId,
                        IsDeleted = obj.IsDeleted,
                    });
                }
                else
                {
                    await _helper.CreateNotification(new NotificationDto()
                    {
                        FromUserId = fromuserId,
                        ToUserId = followRequestDto.ToUserId,
                        NotificationType = NotificationType.FollowRequestAccepted,
                        NotificationTypeId = NotificationTypeId.RequestId,
                        Id = obj.RequestId,
                        IsDeleted = obj.IsDeleted,
                    });
                }
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<CountResponseDto> GetFollowerFollowingPostCountById(long userId)
        {
            int followerCount = await _context.Requests
                .Include(r => r.ToUser)
                .Where(r => r.ToUserId == userId && r.IsAccepted == true && r.IsDeleted == false)
                .CountAsync();

            int followingCount = await _context.Requests
                .Include(r => r.ToUser)
                .Where(r => r.FromUserId == userId && r.IsAccepted == true && r.IsDeleted == false)
                .CountAsync();

            int postCount = await _context.Posts
                .Where(m => m.UserId == userId && m.IsDeleted == false && m.PostTypeId == 4)
                .CountAsync();
            return new CountResponseDto(followerCount, followingCount, postCount);
        }

        public async Task<PaginationResponseModel<RequestListResponseDto>> GetRequestListById(RequestDto<FollowRequestDto> requestDto)
        {
            IQueryable<RequestListResponseDto> data = _context.Requests
                           .Include(m => m.FromUser) // Ensure FromUser is included for UserDTO
                           .Where(m => m.ToUserId == requestDto.Model.userId && m.IsDeleted == false && m.IsAccepted == false)
                           .OrderByDescending(r => r.CreatedDate)
                           .Select(r => new RequestListResponseDto
                           {
                               RequestId = r.RequestId,
                               User = new UserDto
                               {
                                   UserId = r.FromUser.UserId,
                                   UserName = r.FromUser.UserName,
                                   Email = r.FromUser.Email,
                                   Name = r.FromUser.Name,
                                   Bio = r.FromUser.Bio,
                                   Link = r.FromUser.Link,
                                   Gender = r.FromUser.Gender ?? string.Empty,
                                   ProfilePictureName = r.FromUser.ProfilePictureName ?? string.Empty,
                                   ProfilePictureUrl = r.FromUser.ProfilePictureUrl ?? string.Empty,
                                   ContactNumber = r.FromUser.ContactNumber ?? string.Empty,
                                   IsPrivate = r.FromUser.IsPrivate,
                                   IsVerified = r.FromUser.IsVerified,
                                   IsFollower = _context.Requests.Any(req => req.FromUserId == r.FromUser.UserId && req.ToUserId == requestDto.Model.userId && req.IsAccepted == true && req.IsDeleted == false),
                                   IsFollowing = _context.Requests.Any(req => req.FromUserId == requestDto.Model.userId && req.ToUserId == r.FromUser.UserId && req.IsAccepted == true && req.IsDeleted == false),
                                   IsRequest = _context.Requests.Any(req => req.FromUserId == requestDto.Model.userId && req.ToUserId == r.FromUser.UserId && req.IsAccepted == false && req.IsDeleted == false),
                               }
                           });


            List<RequestListResponseDto> requests = await data
                .Skip((requestDto.PageNumber - 1) * requestDto.PageSize)
                .Take(requestDto.PageSize)
                .ToListAsync();

            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / requestDto.PageSize);
            return new PaginationResponseModel<RequestListResponseDto>
            {
                TotalRecord=totalRecords,
                RequiredPage=requiredPages,
                PageNumber=requestDto.PageNumber,
                PageSize=requestDto.PageSize,
                Records=requests
            };
        }

        public async Task<UserDto> GetUserData(long userId)
        {
            long logInUserId = _helper.GetUserIdClaim();
            User user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == userId && !m.IsDeleted)
                        ?? throw new ValidationException(CustomErrorMessage.ExistUser, CustomErrorCode.IsNotExist, new List<ValidationError>
                        {
                    new ValidationError
                    {
                        message = CustomErrorMessage.ExistUser,
                        reference = "userId",
                        parameter = "userId",
                        errorCode = CustomErrorCode.IsNotExist
                    }
                        });

            UserDto userDTO = new()
            {
                UserId = user.UserId,
                UserName = user.UserName,
                Email = user.Email,
                Name = user.Name,
                Bio = user.Bio,
                Link = user.Link,
                DateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                Gender = user.Gender ?? string.Empty,
                ProfilePictureName = user.ProfilePictureName,
                ProfilePictureUrl = user.ProfilePictureUrl,
                ContactNumber = user.ContactNumber,
                IsPrivate = user.IsPrivate,
                IsVerified = user.IsVerified,
                IsFollower = await _context.Requests.AnyAsync(r => r.FromUserId == user.UserId && r.ToUserId == logInUserId && r.IsAccepted == true && r.IsDeleted == false),
                IsFollowing = await _context.Requests.AnyAsync(r => r.FromUserId == logInUserId && r.ToUserId == user.UserId && r.IsAccepted == true && r.IsDeleted == false),
                IsRequest = await _context.Requests.AnyAsync(r => r.FromUserId == logInUserId && r.ToUserId == user.UserId && r.IsAccepted == false && r.IsDeleted == false),
            };

            return userDTO;
        }

        public async Task<PaginationResponseModel<UserDto>> GetUserListByUserName(RequestDto<UserIdRequestDto> requestDto)
        {
            long logInUserId = _helper.GetUserIdClaim();
            IQueryable<UserDto> data = _context.Users
                 .Where(m => m.IsDeleted == false && m.UserId != logInUserId &&
                            (string.IsNullOrEmpty(requestDto.SearchName) ||
                            (m.UserName ?? string.Empty).ToLower().Contains(requestDto.SearchName.ToLower())))
                 .Select(user => new UserDto
                 {
                     UserId = user.UserId,
                     UserName = user.UserName,
                     Email = user.Email,
                     Name = user.Name,
                     Bio = user.Bio,
                     Link = user.Link,
                     DateOfBirth = user.DateOfBirth.HasValue ? user.DateOfBirth.Value.ToString("yyyy-MM-dd") : string.Empty,
                     Gender = user.Gender ?? string.Empty,
                     ProfilePictureName = user.ProfilePictureName,
                     ProfilePictureUrl = user.ProfilePictureUrl,
                     ContactNumber = user.ContactNumber,
                     IsPrivate = user.IsPrivate,
                     IsVerified = user.IsVerified,
                     IsFollower = _context.Requests.Any(r => r.FromUserId == user.UserId && r.ToUserId == logInUserId && r.IsAccepted == true && r.IsDeleted == false),
                     IsFollowing = _context.Requests.Any(r => r.FromUserId == logInUserId && r.ToUserId == user.UserId && r.IsAccepted == true && r.IsDeleted == false),
                     IsRequest = _context.Requests.Any(r => r.FromUserId == logInUserId && r.ToUserId == user.UserId && r.IsAccepted == false && r.IsDeleted == false),
                 });


            int totalRecords = await data.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / requestDto.PageSize);

            // Paginate the data
            List<UserDto> records = await data
                .Skip((requestDto.PageNumber - 1) * requestDto.PageSize)
                .Take(requestDto.PageSize)
                .ToListAsync();

            return new PaginationResponseModel<UserDto>
            {
                TotalRecord = totalRecords,
                PageNumber = requestDto.PageNumber,
                PageSize = requestDto.PageSize,
                RequiredPage = requiredPages,
                Records = records
            };
        }

        public  async Task<GetProfilePhotoDto> GetUserProfilePhoto(long userId)
        {
            User data=await _context.Users.FindAsync(userId);
            GetProfilePhotoDto pf = new GetProfilePhotoDto();
            pf.ProfilePhotoName = data.ProfilePictureName;
           // pf.ProfilePhotoUrl = data.ProfilePictureUrl;        
            return pf;
        }

        public async Task<bool> RequestAcceptOrCancel(long requestId, string acceptType)
        {
            Request request = await _context.Requests.FirstOrDefaultAsync(m => m.RequestId == requestId && m.IsDeleted == false)
                ?? throw new ValidationException(CustomErrorMessage.ExitsRequest, CustomErrorCode.IsNotRequest, new List<ValidationError>
                   {
                        new ValidationError
                        {
                            message = CustomErrorMessage.ExitsRequest,
                            reference = "UserName",
                            parameter = "UserName",
                            errorCode = CustomErrorCode.IsNotRequest
                        }
                   });

            if (acceptType == "Accept")
            {
                request.IsAccepted = true;
                request.ModifiedDate = DateTime.Now;
            }
            else if (acceptType == "Cancle")
            {
                request.IsAccepted = false;
                request.IsDeleted = true;
                request.ModifiedDate= DateTime.Now;
            }
            await _helper.CreateNotification(new NotificationDto()
            {
                FromUserId = request.FromUserId,
                ToUserId = request.ToUserId,
                NotificationType = NotificationType.FollowRequestAccepted,
                NotificationTypeId = NotificationTypeId.RequestId,
                Id = requestId,
                IsDeleted = request.IsDeleted,
            });
            _context.Requests.Update(request);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<ProfilePhotoResponseDto> UploadProfilePhoto(IFormFile ProfilePhoto, long userId)
        {
            User user = await _context.Users.FirstOrDefaultAsync(m => m.UserId == userId && m.IsDeleted != true) ??
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

            IFormFile file = ProfilePhoto;

            // Delete the old profile photo file if it exists
            if (!string.IsNullOrEmpty(user.ProfilePictureUrl) && System.IO.File.Exists(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl)))
            {
                System.IO.File.Delete(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", user.ProfilePictureUrl));
            }

            if (ProfilePhoto != null)
            {
                string userID = userId.ToString();

                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userID, "ProfilePhoto");

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                string filePath = Path.Combine(path, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(fileStream);
                }

                user.ProfilePictureUrl = Path.Combine("content", "User", userID, "ProfilePhoto", fileName);
                user.ProfilePictureName = fileName;
            }
            else
            {
                user.ProfilePictureUrl = null;
                user.ProfilePictureName = null;
            }
            user.ModifiedDate = DateTime.Now;
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            ProfilePhotoResponseDto photoResponseDTO = new()
            {
                ProfilePhotoName = user.ProfilePictureName,
                ProfilePhotoUrl = user.ProfilePictureUrl,
                UserId = user.UserId,
            };
            return photoResponseDTO;
        }

        public string GetContentType(string fileExtension)
        {
            return fileExtension switch
            {
                "jpg" or "jpeg" => "image/jpeg",
                "png" => "image/png",
                "mp4" => "video/mp4",
                _ => "application/octet-stream",
            };
        }

       
    }

}

