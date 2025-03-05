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
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly Helper _helper;

        public PostService(AppDbContext context, Helper helper)
        {
            _context = context;
            _helper = helper;
        }

        public async Task<bool> CommentPost(CommentPostDto commentPostDto)
        {
            Comment comment = new()
            {
                UserId = commentPostDto.UserId,
                PostId = commentPostDto.PostId,
                CommentText = commentPostDto.CommentText,
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                IsDeleted = false,
            };
            await _context.Comments.AddAsync(comment);
            await _context.SaveChangesAsync();

            long toUserId = _context.Posts.FirstOrDefaultAsync(m => m.PostId == commentPostDto.PostId && !m.IsDeleted).Result?.UserId ?? 0;
            if (toUserId != commentPostDto.UserId)
            {
                await _helper.CreateNotification(new NotificationDto()
                {
                    FromUserId = commentPostDto.UserId,
                    ToUserId = toUserId,
                    NotificationType = NotificationType.PostCommented,
                    NotificationTypeId = NotificationTypeId.CommentId,
                    Id = comment.CommentId,
                    IsDeleted = comment.IsDeleted,
                    PostId = commentPostDto.PostId,
                    modifiedDate= DateTime.Now,
                });
            }
            return true;

        }

        public async Task<PostResponseDTO> CreatePost(CreatePostDto createPostDto)
        {
            long UserId = _helper.GetUserIdClaim();
            Post post = await _context.Posts.FirstOrDefaultAsync(m => m.PostId == createPostDto.PostId && m.IsDeleted == false) ?? new();

            post.Caption = createPostDto.Caption;
            post.Location = createPostDto.Location;

            if (createPostDto.PostId > 0)
            {
                post.ModifiedDate = DateTime.Now;
                _context.Posts.Update(post);
            }
            else
            {
                post.CreatedDate = DateTime.Now;
                post.ModifiedDate = DateTime.Now;
                post.UserId = UserId;
                if (createPostDto.PostType == "Post")
                {
                    post.PostTypeId = 4;
                }
                else if (createPostDto.PostType == "Reel")
                {
                    post.PostTypeId = 3;
                }

                await _context.Posts.AddAsync(post);
            }
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                
                throw; 
            }
            List<Media> medias = new();
            List<PostMapping> postMappings = new();
            if (createPostDto.PostId == 0)
            {
                foreach (var file in createPostDto.File)
                {
                    string mediaType = Path.GetExtension(file.FileName).TrimStart('.');
                    string userId = UserId.ToString();
                    string path = "";

                    if (createPostDto.PostType == "Post")
                    {
                        path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId, "Post");
                    }
                    else if (createPostDto.PostType == "Reel")
                    {
                        path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId, "Reel");
                    }

                    if (!Directory.Exists(path))
                    {
                        Directory.CreateDirectory(path ?? string.Empty);
                    }

                    string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
                    string filePath = Path.Combine(path ?? string.Empty, fileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    string mediaURL = Path.Combine("content", "User", userId, createPostDto.PostType ?? string.Empty, fileName);

                    int mediaTypeId = 0;

                    if (file.ContentType.Contains("image"))
                    {
                        mediaTypeId = 1;
                    }
                    else if (file.ContentType.Contains("video"))
                    {
                        mediaTypeId = 2;
                    }
                    PostMapping postMapping = new()
                    {
                        PostId = post.PostId,
                        MediaTypeId = mediaTypeId,
                        MediaUrl = mediaURL,
                        MediaName = fileName,
                        CreatedDate = DateTime.Now,
                        ModifiedDate = DateTime.Now,
                    };

                    postMappings.Add(postMapping);
                    await _context.SaveChangesAsync();
                    Media media = new()
                    {
                        PostMappingId = postMapping.PostMappingId,
                        MediaType = file.ContentType,
                        MediaURL = mediaURL,
                        MediaName = fileName,
                    };
                    medias.Add(media);
                }

                await _context.PostMappings.AddRangeAsync(postMappings);
                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw;
                }
            }
            PostResponseDTO responseDTO = new()
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Caption = post.Caption,
                Location = post.Location,
                PostType = createPostDto.PostType,
                Medias = medias
            };
            return responseDTO;
        }

        public async Task<bool> DetelePostById(long postId)
        {
            Post? post = await _context.Posts.FirstOrDefaultAsync(m => m.PostId == postId && m.IsDeleted == false);
            if (post != null)
            {
                post.IsDeleted = true;
                post.ModifiedDate = DateTime.Now;

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                return true;
            }
            return false;
        }

        public async Task<bool> DetelePostComment(long commentId)
        {
            Comment? comment = await _context.Comments.FirstOrDefaultAsync(m => m.CommentId == commentId && m.IsDeleted == false);
            if (comment != null)
            {
                comment.IsDeleted = true;
                comment.CommentText = "";
                comment.ModifiedDate = DateTime.Now;

                _context.Comments.Update(comment);
                await _context.SaveChangesAsync();

                await _helper.CreateNotification(new NotificationDto()
                {
                    FromUserId = comment.UserId,
                    ToUserId = _context.Posts.FirstOrDefaultAsync(m => m.PostId == comment.PostId && !m.IsDeleted).Result?.UserId ?? 0,
                    NotificationType = NotificationType.PostCommented,
                    NotificationTypeId = NotificationTypeId.CommentId,
                    Id = comment.CommentId,
                    IsDeleted = comment.IsDeleted,
                    PostId = comment.PostId,
                });
                return true;
            }
            return false;

        }

        

        public async Task<PaginationResponseModel<PostResponseDTO>> GetPostAndReelListById(RequestDto<PostListRequestDto> requestDto)
        {

            IQueryable<PostResponseDTO> posts = _context.Posts
                    .Include(m => m.Likes)
                    .Include(m => m.PostMappings).Include(m => m.Comments).Include(m => m.User)
                    .Where(m => m.IsDeleted == false && (requestDto.Model.PostType == "Post" ? m.PostTypeId == 4 : m.PostTypeId == 3) && m.UserId == requestDto.Model.UserId)
                    .OrderByDescending(p => p.CreatedDate)
                    .Select(post => new PostResponseDTO
                 {
                            PostId = post.PostId,
                            UserId = post.UserId,
                            UserName = post.User.UserName,
                            ProfilePhotoName = post.User.ProfilePictureName,
                            Caption = post.Caption,
                            Location = post.Location,
                            PostType = post.PostTypeId == 3 ? "Reel" : "Post",
                            Medias = post.PostMappings.Select(m => new Media
                            {
                                PostMappingId = m.PostMappingId,
                                MediaType = m.MediaTypeId == 1 ? "Images" : "Video",
                                MediaURL = m.MediaUrl,
                                MediaName = m.MediaName
                            }).ToList(),
                            PostLikes = post.Likes.Where(l => l.IsDeleted == false).Select(l => new PostLike
                            {
                                LikeId = l.LikeId,
                                UserId = l.UserId,
                                Avtar = l.User.ProfilePictureName,
                                UserName = l.User.UserName
                            }).ToList(),
                            PostComments = post.Comments.Where(l => l.IsDeleted == false).Select(c => new PostComment
                            {
                                CommentId = c.CommentId,
                                UserId = c.UserId,
                                CommentText = c.CommentText,
                                Avtar = c.User.ProfilePictureName,
                                UserName = c.User.UserName
                            }).ToList()

                        });


            int totalRecords = await posts.CountAsync();
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / requestDto.PageSize);

            List<PostResponseDTO> postResponses = await posts
                .Skip((requestDto.PageNumber - 1) * requestDto.PageSize)
                .Take(requestDto.PageSize)
                .ToListAsync();

            return new PaginationResponseModel<PostResponseDTO>
            {
                TotalRecord = totalRecords,
                PageSize = requestDto.PageSize,
                PageNumber = requestDto.PageNumber,
                RequiredPage = requiredPages,
                Records = postResponses
            };
        }

        public async Task<PostResponseDTO> GetPostById(long postId, string postType)
        {
            Post post = await _context.Posts
    .Include(p => p.PostMappings)
        .ThenInclude(pm => pm.MediaType)
    .Include(p => p.Likes)
        .ThenInclude(l => l.User)
    .Include(p => p.Comments)
        .ThenInclude(c => c.User)
    .Include(p => p.User)
    .FirstOrDefaultAsync(p => p.PostId == postId && !p.IsDeleted &&
                              (postType == "Post" ? p.PostTypeId == 4 : p.PostTypeId == 3))
    ??
    throw new ValidationException(CustomErrorMessage.NullPostId, CustomErrorCode.NullPostId, new List<ValidationError>
    {
        new ValidationError
        {
         message = CustomErrorMessage.NullPostId,
        reference = "postid",
        parameter = "postid",
        errorCode = CustomErrorCode.NullPostId
        }
    });

            return new PostResponseDTO
            {
                PostId = post.PostId,
                UserId = post.UserId,
                UserName = post.User.UserName,
                ProfilePhotoName = post.User.ProfilePictureName,
                Caption = post.Caption,
                Location = post.Location,
                PostType = post.PostTypeId == 3 ? "Reel" : "Post",
                Medias = post.PostMappings.Select(m => new Media
                {
                    PostMappingId = m.PostMappingId,
                    MediaType = m.MediaTypeId == 1 ? "Images" : "Video",
                    MediaURL = m.MediaUrl,
                    MediaName = m.MediaName
                }).ToList(),
                PostLikes = post.Likes.Where(l => l.IsDeleted == false).Select(l => new PostLike
                {
                    LikeId = l.LikeId,
                    UserId = l.UserId,
                    Avtar = l.User.ProfilePictureName,
                    UserName = l.User.UserName
                }).ToList(),
                PostComments = post.Comments.Where(l => l.IsDeleted == false).Select(c => new PostComment
                {
                    CommentId = c.CommentId,
                    UserId = c.UserId,
                    CommentText = c.CommentText,
                    Avtar = c.User.ProfilePictureName,
                    UserName = c.User.UserName
                }).ToList()
            };
        }

        public async Task<PaginationResponseModel<PostResponseDTO>> GetPostListById(PaginationRequestDto paginationRequestDto)
        {
            long UserId = _helper.GetUserIdClaim();
            List<long> requestUserIds = await _context.Requests
                .Where(m => m.FromUserId == UserId && !m.IsDeleted && m.IsAccepted)
                .Select(m => m.ToUserId)
                .ToListAsync();
            List<long> userUserIds = await _context.Users
                .Where(u => !u.IsDeleted && !u.IsPrivate)
                .Select(u => u.UserId)
                .ToListAsync();
            HashSet<long> combinedUserIds = requestUserIds.Union(userUserIds).ToHashSet();
            
            List<PostResponseDTO> postList = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.PostMappings)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => !p.IsDeleted) 
                .OrderByDescending(p => p.CreatedDate)
                .Select(post => new PostResponseDTO
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    UserName = post.User.UserName,
                    ProfilePhotoName = post.User.ProfilePictureName,
                    Caption = post.Caption,
                    Location = post.Location,
                    PostType = post.PostTypeId == 3 ? "Reel" : "Post",
                    Medias = post.PostMappings.Select(m => new Media
                    {
                        PostMappingId = m.PostMappingId,
                        MediaType = m.MediaTypeId == 1 ? "Images" : "Video",
                        MediaURL = m.MediaUrl,
                        MediaName = m.MediaName
                    }).ToList(),
                    PostLikes = post.Likes.Where(l => !l.IsDeleted).Select(l => new PostLike
                    {
                        LikeId = l.LikeId,
                        UserId = l.UserId,
                        Avtar = l.User.ProfilePictureName,
                        UserName = l.User.UserName
                    }).ToList(),
                    PostComments = post.Comments.Where(c => !c.IsDeleted).Select(c => new PostComment
                    {
                        CommentId = c.CommentId,
                        UserId = c.UserId,
                        CommentText = c.CommentText,
                        Avtar = c.User.ProfilePictureName,
                        UserName = c.User.UserName
                    }).ToList()
                })
                .ToListAsync();
            List<PostResponseDTO> filteredPosts = postList
                .Where(p => combinedUserIds.Contains(p.UserId))
                .ToList();
            int totalRecords = filteredPosts.Count;
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / paginationRequestDto.PageSize);
            List<PostResponseDTO> paginatedPosts = filteredPosts
                .Skip((paginationRequestDto.PageNumber - 1) * paginationRequestDto.PageSize)
                .Take(paginationRequestDto.PageSize)
                .ToList();
            return new PaginationResponseModel<PostResponseDTO>
            {
                TotalRecord = totalRecords,
                PageSize = paginationRequestDto.PageSize,
                PageNumber = paginationRequestDto.PageNumber,
                RequiredPage = requiredPages,
                Records = paginatedPosts
            };
        }

        public async Task<bool> LikeAndUnlikePost(LikePostDto likePostDto)
        {
            Like? like = await _context.Likes.FirstOrDefaultAsync(m => m.UserId == likePostDto.UserId && m.PostId == likePostDto.PostId);
            Like obj = like ?? new();

            obj.UserId = likePostDto.UserId;
            obj.PostId = likePostDto.PostId;
            obj.IsLike = likePostDto.IsLike;
            obj.ModifiedDate = DateTime.Now;

            if (like != null)
            {
                like.IsDeleted = !likePostDto.IsLike;
                like.ModifiedDate = DateTime.Now;

                _context.Likes.Update(like);
            }
            else
            {
                obj.CreatedDate = DateTime.Now;
                obj.ModifiedDate = DateTime.Now;
                await _context.Likes.AddAsync(obj);
            }
            await _context.SaveChangesAsync();

            long toUserId = _context.Posts.FirstOrDefaultAsync(m => m.PostId == likePostDto.PostId && !m.IsDeleted).Result?.UserId ?? 0;
            if (toUserId != likePostDto.UserId)
            {
                await _helper.CreateNotification(new NotificationDto()
                {
                    FromUserId = likePostDto.UserId,
                    ToUserId = toUserId,
                    NotificationType = NotificationType.PostLiked,
                    NotificationTypeId = NotificationTypeId.LikeId,
                    Id = obj.LikeId,
                    IsDeleted = obj.IsDeleted,
                    PostId = likePostDto.PostId,
                    modifiedDate = DateTime.Now
                });
            }
            return true;
        }

        public async Task<bool> SaveOrUnSavePost(long postId, bool IsSaved)
        {
            long UserId = _helper.GetUserIdClaim();
            List<long> FollowingUserIds = await _context.Requests
               .Where(m => m.FromUserId == UserId && !m.IsDeleted && m.IsAccepted)
               .Select(m => m.ToUserId)
               .ToListAsync();
            List<long> PostIds=null;
            foreach (var item in FollowingUserIds)
            {
                PostIds =await _context.Posts.Where(x => x.UserId == item && x.IsDeleted == false).Select(x=>x.PostId).ToListAsync();

                foreach (var id in PostIds)
                {
                    var data = await _context.Posts.Where(x => x.PostId == id).FirstOrDefaultAsync();
                    if (data != null && data.PostId == postId && data.IsDeleted == false)
                    {
                        data.IsSaved = IsSaved;
                        data.ModifiedDate = DateTime.Now;
                        _context.Posts.Update(data);
                    }
                }

            }
           
            var a=await _context.SaveChangesAsync();
            return a>0?true:false;
        }

        public async Task<PaginationResponseModel<PostResponseDTO>> GetAllSavedPost()
        {
            long UserId = _helper.GetUserIdClaim();
            List<long> FollowingUserIds = await _context.Requests
               .Where(m => m.FromUserId == UserId && !m.IsDeleted && m.IsAccepted)
               .Select(m => m.ToUserId)
               .ToListAsync();

            List<PostResponseDTO> postList = await _context.Posts
                .Include(p => p.User)
                .Include(p => p.PostMappings)
                .Include(p => p.Likes)
                .Include(p => p.Comments)
                .Where(p => !p.IsDeleted && p.IsSaved==true)
                .OrderByDescending(p => p.CreatedDate)
                .Select(post => new PostResponseDTO
                {
                    PostId = post.PostId,
                    UserId = post.UserId,
                    UserName = post.User.UserName,
                    ProfilePhotoName = post.User.ProfilePictureName,
                    Caption = post.Caption,
                    Location = post.Location,
                    PostType = post.PostTypeId == 3 ? "Reel" : "Post",
                    Medias = post.PostMappings.Select(m => new Media
                    {
                        PostMappingId = m.PostMappingId,
                        MediaType = m.MediaTypeId == 1 ? "Images" : "Video",
                        MediaURL = m.MediaUrl,
                        MediaName = m.MediaName
                    }).ToList(),
                    PostLikes = post.Likes.Where(l => !l.IsDeleted).Select(l => new PostLike
                    {
                        LikeId = l.LikeId,
                        UserId = l.UserId,
                        Avtar = l.User.ProfilePictureName,
                        UserName = l.User.UserName
                    }).ToList(),
                    PostComments = post.Comments.Where(c => !c.IsDeleted).Select(c => new PostComment
                    {
                        CommentId = c.CommentId,
                        UserId = c.UserId,
                        CommentText = c.CommentText,
                        Avtar = c.User.ProfilePictureName,
                        UserName = c.User.UserName
                    }).ToList()
                })
                .ToListAsync();
            List<PostResponseDTO> filteredPosts = postList
                .Where(p=>FollowingUserIds.Contains(p.UserId))
                .ToList();
            int totalRecords = filteredPosts.Count;
            int requiredPages;
            if (totalRecords == 0)
            {
                requiredPages = 0;
            }
            else
            {
                requiredPages = (int)Math.Ceiling((decimal)totalRecords / totalRecords);
            }
            List<PostResponseDTO> paginatedPosts = filteredPosts
                .Skip((totalRecords - 1) * totalRecords)
                .Take(totalRecords)
                .ToList();
            return new PaginationResponseModel<PostResponseDTO>
            {
                TotalRecord = totalRecords,
                PageSize = totalRecords,
                PageNumber = totalRecords,
                RequiredPage = requiredPages,             
                Records = paginatedPosts
            
               
            };

        }
    }
}
