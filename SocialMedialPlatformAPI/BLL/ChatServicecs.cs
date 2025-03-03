using Microsoft.EntityFrameworkCore;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Models;
namespace SocialMedialPlatformAPI.BLL
{
    public class ChatServicecs:IChatService
    {
        private readonly AppDbContext _context;

        public ChatServicecs(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PaginationResponseModel<ResponseMessageDto>> GetAllMessage(long UserId)
        {
            
                List<ResponseMessageDto> message = await _context.Chats
                    .Include(x=>x.Messages)                   
                    .Select(m => new ResponseMessageDto
                    {
                        FromUserId = m.FromUserId,
                        MessageList=m.Messages.Where(ml=>ml.IsDeleted==false).Select(ml=>new GetMessageList
                        {
                            Message=ml.MessageText,
                        }).ToList()
                    }).ToListAsync();              

                List<ResponseMessageDto> filteredMessage = message.Where(p=>p.FromUserId!=UserId).ToList();
                
                int totalRecords = filteredMessage.Count;
                if(totalRecords !=0)
            {
                var data = await _context.Messages.Where(x => x.ToUserId == UserId).ToListAsync();
                foreach(var d in data)
                {
                    d.IsSeen = true;
                    d.ModifiedDate = DateTime.Now;
                    _context.Messages.Update(d);
                }
                await _context.SaveChangesAsync();
            }
                int requiredPages = (int)Math.Ceiling((decimal)totalRecords / totalRecords);
                List<ResponseMessageDto> paginatedMessage = filteredMessage
                    .Skip((totalRecords - 1) * totalRecords)
                    .Take(totalRecords)
                    .ToList();
                return new PaginationResponseModel<ResponseMessageDto>
                {
                    TotalRecord = totalRecords,
                    PageSize = totalRecords,
                    PageNumber = totalRecords,
                    RequiredPage = requiredPages,
                    Records = paginatedMessage
                };
           
        }

        public async Task<PaginationResponseModel<ResponseMessageDto>> GetMessageById(long FromUserId)
        {
            List<ResponseMessageDto> message = await _context.Chats
                    .Include(x => x.Messages)
                    .Where(x=>x.FromUserId==FromUserId)
                    .Select(m => new ResponseMessageDto
                    {
                        FromUserId = m.FromUserId,
                        MessageList = m.Messages.Where(ml => ml.IsDeleted == false && ml.FromUserId==FromUserId).Select(ml => new GetMessageList
                        {
                            Message = ml.MessageText,
                        }).ToList()
                    }).ToListAsync();
            int totalRecords = message.Count;
            
            if (totalRecords != 0)
            {
                var data = await _context.Messages.Where(x => x.FromUserId == FromUserId).ToListAsync();
                foreach (var d in data)
                {
                    d.IsSeen = true;
                    d.ModifiedDate = DateTime.Now;
                    _context.Messages.Update(d);
                }
                await _context.SaveChangesAsync();
            }
            int requiredPages = (int)Math.Ceiling((decimal)totalRecords / totalRecords);
            
            List<ResponseMessageDto> paginatedMessage = message
                .Skip((totalRecords - 1) * totalRecords)
                .Take(totalRecords)
                .ToList();
           
            return new PaginationResponseModel<ResponseMessageDto>
            {
                TotalRecord = totalRecords,
                PageSize = totalRecords,
                PageNumber = totalRecords,
                RequiredPage = requiredPages,
                Records = message
            };

        }

        public async Task<bool> RemoveAllMessageById(long FromUserId)
        {
            var data = await _context.Messages.Where(x => x.FromUserId == FromUserId).ToListAsync();
            foreach(var d in data)
            {
                d.IsDeleted = true;
                d.ModifiedDate = DateTime.Now;
                _context.Messages.Update(d);
            }
            var a=await _context.SaveChangesAsync();
            return a>0?true:false;
        }

        public async Task<bool> RemoveAllMessages(long userId)
        {

            var data = await _context.Messages.Where(x => x.ToUserId == userId).ToListAsync();
            foreach (var d in data)
            {
                d.IsDeleted = true;
                d.ModifiedDate = DateTime.Now;
                _context.Messages.Update(d);
            }
            var a = await _context.SaveChangesAsync();
            return a > 0 ? true : false;

        }

        public async Task<bool> RemoveMessagesById(long userId,long messageId)
        {
            var data=await _context.Messages.Where(x=>x.ToUserId==userId && x.MessageId==messageId).FirstOrDefaultAsync();
            if (data != null)
            {
                data.IsDeleted = true;
                data.ModifiedDate = DateTime.Now;
                _context.Messages.Update(data);
            }
            var a=await _context.SaveChangesAsync();
            return a>0?true:false;
        }

        public async Task<bool> SendMessage(long userId, SendMessageDto sendMessageDto)
        {
            var data = _context.Chats.Where(x => x.FromUserId == userId).FirstOrDefault();
            if (data == null)
            {
                Chat ch = new Chat();
                ch.FromUserId = userId;
                ch.ToUserId = sendMessageDto.toUserId;
                ch.CreatedDate = DateTime.Now;
                ch.ModifiedDate = DateTime.Now;
                ch.IsDeleted = false;
                await _context.AddAsync(ch);
                var a = await _context.SaveChangesAsync();
                var d = _context.Chats.Where(x => x.FromUserId == userId).FirstOrDefault();
                if (a > 0)
                {
                    Message msg = new Message();
                    msg.FromUserId = userId;
                    msg.ToUserId = d.ToUserId;
                    msg.ChatId = d.ChatId;
                    msg.MessageText = sendMessageDto.Message;
                    msg.CreatedDate = DateTime.Now;
                    msg.ModifiedDate = DateTime.Now;
                    msg.IsDeleted = false;
                    msg.IsDelivered = true;
                    msg.IsSeen = false;
                    await _context.AddAsync(msg);
                    await _context.SaveChangesAsync();
                }
                return true;
            }
            if (data.ToUserId == sendMessageDto.toUserId && data.FromUserId == userId)
            {
                data.ModifiedDate = DateTime.Now;
                _context.Chats.Update(data);
                await _context.SaveChangesAsync();

                Message msg = new Message();
                msg.FromUserId = userId;
                msg.ToUserId = data.ToUserId;
                msg.ChatId = data.ChatId;
                msg.MessageText = sendMessageDto.Message;
                msg.CreatedDate = DateTime.Now;
                msg.ModifiedDate = DateTime.Now;
                msg.IsDeleted = false;
                msg.IsDelivered = true;
                msg.IsSeen = false;
                await _context.Messages.AddAsync(msg);
                await _context.SaveChangesAsync();

                return true;

            }
          
            return false;
        }
    }
}
