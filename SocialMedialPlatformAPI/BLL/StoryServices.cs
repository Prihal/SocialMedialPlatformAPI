using Microsoft.EntityFrameworkCore;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Helpers;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Models;
using static SocialMedialPlatformAPI.Utils.Enum;

namespace SocialMedialPlatformAPI.BLL
{
    public class StoryServices : IStoryServicecs
    {
        private readonly AppDbContext _context;
        private readonly Helper _helper;      
        public StoryServices(AppDbContext context,Helper helper)
        {
            _context = context;
            _helper = helper;
        }
        public async Task<bool> AddStory(long userId, StoryDto storyDto)
        {
            Story story = new Story();
            story.UserId = userId;
            string mediaType = Path.GetExtension(storyDto.File.FileName).TrimStart('.');
            string UserId = userId.ToString();
            string path = "";
            path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", UserId, "Story");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path ?? string.Empty);
            }
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(storyDto.File.FileName);
            string filePath = Path.Combine(path ?? string.Empty, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await storyDto.File.CopyToAsync(fileStream);
            }

            string mediaURL = Path.Combine("content", "User",UserId,"Story", fileName);

            int mediaTypeId = 0;

            if (storyDto.File.ContentType.Contains("image"))
            {
                mediaTypeId = 1;
            }
            else if (storyDto.File.ContentType.Contains("video"))
            {
                mediaTypeId = 2;
            }
            else if (storyDto.File.ContentType.Contains("Post"))
            {
                mediaTypeId = 3;
            }
            else
            {
                mediaTypeId = 4;
            }
            story.StoryTypeId = mediaTypeId;
            story.StoryUrl = filePath;
            story.StoryName = fileName;  
            DateTime currentTime = DateTime.Now;
            TimeSpan duration = currentTime - story.CreatedDate;
            story.StoryDuration = 0;
            story.Caption=storyDto.Caption;
            story.IsHighlighted = false;
            story.IsDeleted = false;
            story.CreatedDate = DateTime.Now;
            story.ModifiedDate = DateTime.Now;
            await _context.Stories.AddAsync(story); 
            var a = await _context.SaveChangesAsync();
            return a > 0 ? true : false;  
        }
        public void ChangeStoryDuration()
        {
            long userId = _helper.GetUserIdClaim();
            var data = _context.Stories.ToList();
            
            foreach (var item in data)
            {
                if (item.IsDeleted == false)
                {
                    item.StoryDuration = item.StoryDuration;
                    item.ModifiedDate = item.ModifiedDate;
               
                    var currentTime = DateTime.Now;
                    var duration = currentTime - item.CreatedDate;
                    item.StoryDuration = (int)duration.TotalHours;
                    item.ModifiedDate = currentTime;
                }
                if (item.StoryDuration == 24 || item.StoryDuration>24) { 
                    item.IsDeleted = true;
                    item.ModifiedDate = DateTime.Now;
                }
                _context.Stories.Update(item);
            }
            _context.SaveChanges();             
        }

        public async Task<bool> DeleteStory(long userId, long storyId)
        {
            ChangeStoryDuration();
            var data = await _context.Stories.Where(x => x.StoryId == storyId && x.UserId == userId).FirstOrDefaultAsync();
            if (data != null)
            {
                data.IsDeleted = true;
                data.ModifiedDate = DateTime.Now;
                _context.Stories.Update(data);
            }
            var a=await _context.SaveChangesAsync();
            
            return a>0?true:false;
        }

        public async Task<PaginationResponseModel<GetAllArchiveStoryDto>> GetAllArchiveStory(long userId)
        {
            ChangeStoryDuration();
           var stories = await _context.Stories
           .Where(s => s.UserId==userId && s.StoryDuration>24)
           .GroupBy(s => s.UserId)
           .Select(g => new GetAllArchiveStoryDto
           {
               UserId = g.Key,
               Stories = g.Select(s => new GetArchiveStorys
               {
                   UserId = s.UserId,
                   StoryUrl = s.StoryUrl,
                   StoryName = s.StoryName,
                   CreateDate=s.CreatedDate
               }).OrderBy(x=>x.CreateDate).ToList()
           })
           .ToListAsync();
            

            int totalRecords = stories.Count;
            int requiredPages;
            if (totalRecords == 0)
            {
                requiredPages = 0;
            }
            else
            {
                requiredPages = (int)Math.Ceiling((decimal)totalRecords / totalRecords);
                List<GetAllArchiveStoryDto> paginatedMessage = stories
                    .Skip((totalRecords - 1) * totalRecords)
                    .Take(totalRecords)
                    .ToList();
            }
            return new PaginationResponseModel<GetAllArchiveStoryDto>
            {
                TotalRecord = totalRecords,
                PageSize = totalRecords,
                PageNumber = totalRecords,
                RequiredPage = requiredPages,
                Records = stories
            };

        }

        public async Task<PaginationResponseModel<GetAllStoryDto>> GetAllStory(long userId)
        {
            ChangeStoryDuration();

            var followingUserIds = await _context.Requests
         .Where(f => f.FromUserId == userId && f.IsAccepted == true) 
         .Select(f => f.ToUserId) 
         .ToListAsync(); 
     var stories = await _context.Stories
    .Where(s => s.IsDeleted == false)
    .GroupBy(s => s.UserId)
    .Select(g => new GetAllStoryDto
    {
        UserId = g.Key,
        Stories = g.Select(s => new GetStorys
        {
            UserId = s.UserId,
            StoryUrl = s.StoryUrl,
            StoryName = s.StoryName,
            StoryDuration = (int)s.StoryDuration
        }).ToList()
    })
    .ToListAsync();        
         List<GetAllStoryDto> allStories= stories.Where(x=>followingUserIds.Contains(x.UserId)).ToList();

            int totalRecords = allStories.Count;
            int requiredPages;
            if (totalRecords == 0)
            {
                requiredPages = 0;
            }
            else
            {
                requiredPages = (int)Math.Ceiling((decimal)totalRecords / totalRecords);
                List<GetAllStoryDto> paginatedMessage = allStories
                    .Skip((totalRecords - 1) * totalRecords)
                    .Take(totalRecords)
                    .ToList();
            }
            return new PaginationResponseModel<GetAllStoryDto>
            {
                TotalRecord = totalRecords,
                PageSize = totalRecords,
                PageNumber = totalRecords,
                RequiredPage = requiredPages,
                Records = allStories
            };

        }

        public async Task<PaginationResponseModel<GetStoryDto>> GetStory(long userId)
        {
            ChangeStoryDuration();
            List<GetStory> stories = await _context.Stories
           .Where(x => x.UserId == userId && x.IsDeleted == false &&  x.StoryDuration!=24)
             .Select(sl => new GetStory
             {
        StoryName = sl.StoryName,
        StoryUrl = sl.StoryUrl,
        StoryDuration = (int)sl.StoryDuration
         })
         .ToListAsync();

            List<GetStoryDto> result = new List<GetStoryDto>
            {
              new GetStoryDto
             {
              UserId = userId,
             Stories = stories
             }
        };

            int totalRecords = stories.Count;
            int requiredPages;
            if (totalRecords == 0) { requiredPages = 1; }
            else
            {
                requiredPages = (int)Math.Ceiling((decimal)totalRecords / totalRecords);
                List<GetStory> paginatedMessage = stories
                    .Skip((totalRecords - 1) * totalRecords)
                    .Take(totalRecords)
                    .ToList();
            }
            return new PaginationResponseModel<GetStoryDto>
            {
                TotalRecord = totalRecords,
                PageSize = totalRecords,
                PageNumber = totalRecords,
                RequiredPage = requiredPages,
                Records = result
            };

        }

        public async Task<bool> RemoveHighlight(long userId, long storyId)
        {
            ChangeStoryDuration();
            var data = await _context.Stories.Where(x => x.StoryId == storyId && x.UserId == userId).FirstOrDefaultAsync();
            if (data != null && data.IsDeleted == false)
            {
                data.IsHighlighted = false;
                data.ModifiedDate = DateTime.Now;
                _context.Stories.Update(data);
            }
            var a = await _context.SaveChangesAsync();
            return a > 0 ? true : false;
        }

        public async Task<bool> SetHighlight(long userId, long storyId)
        {
            ChangeStoryDuration();
            var data = await _context.Stories.Where(x => x.StoryId == storyId && x.UserId == userId).FirstOrDefaultAsync();
            if (data != null && data.IsDeleted == false)
            {
                data.IsHighlighted = true;
                data.ModifiedDate = DateTime.Now;
                _context.Stories.Update(data);
            }
            var a= await _context.SaveChangesAsync();
            return a>0?true:false;
        }
    }
}
