using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using SocialMedialPlatformAPI.Common;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Helpers;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Utils;

namespace SocialMedialPlatformAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IChatService _chatSevice;
        private readonly Helper _helper;
        private readonly IValidationService _validationService;
        private readonly ResponseHandler _responseHandler;

        public ChatController(IChatService chatSevice,Helper helper,IValidationService validationService,ResponseHandler responseHandler)
        {
            _chatSevice = chatSevice;
            _helper = helper;
            _validationService = validationService;
            _responseHandler = responseHandler;
        }

        [HttpPost("SendMessage")]
        public async Task<ActionResult> SendMessage(SendMessageDto sendMessageDto)
        {

            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateUserId(userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationSendMessage, ""));
                }
                bool a = await _chatSevice.SendMessage(userId, sendMessageDto);
                if (a==false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.SendMessageError, CustomErrorMessage.SendMessageError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.SendMessage,sendMessageDto));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.SendMessageError, ex.Message, ""));
            }
        }

        [HttpGet("GetAllMessage")]
        public async Task<ActionResult> GetAllMessage()
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateUserId(userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationGetMessage, ""));
                }
                PaginationResponseModel<ResponseMessageDto> responseDto = await _chatSevice.GetAllMessage(userId);
                if (responseDto.Records == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.GetMessageError, CustomErrorMessage.GetMessageError, ""));
                }
                
                return Ok(_responseHandler.Success(CustomErrorMessage.GetMessage, responseDto));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.GetMessageError, ex.Message, ""));
            }
        }

        [HttpGet("GetMessageById")]
        public async Task<ActionResult> GetMessageById(long FromUserId)
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                
                List<ValidationError> errors = _validationService.ValidateUserId(FromUserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationGetMessage, ""));
                }
                if (userId == FromUserId)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationGetMessageId, ""));
                }
                PaginationResponseModel<ResponseMessageDto> responseModel=await _chatSevice.GetMessageById(FromUserId);
                if (responseModel.Records == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.GetMessageError, CustomErrorMessage.GetMessageError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetMessage, responseModel));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.GetMessageError, ex.Message, ""));
            }
        }

        [HttpPost("RemoveAllMessageById")]
        public async Task<ActionResult> RemoveAllMessageById(long FromUserId)
        {
            try
            {
                long userId = _helper.GetUserIdClaim();

                List<ValidationError> errors = _validationService.ValidateUserId(FromUserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRemoveMessage, ""));
                }
                if (userId == FromUserId)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationGetMessageId, ""));
                }
                var a = await _chatSevice.RemoveAllMessageById(FromUserId);
                if (a == false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRemoveMessage, CustomErrorMessage.RemoveErrorMessage, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.RemoveMessage,a));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRemoveMessage, ex.Message, ""));
            }
        }

        [HttpPost("RemoveAllMessages")]
        public async Task<ActionResult> RemoveAllMessages()
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateUserId(userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRemoveMessage, ""));
                }
                var a = await _chatSevice.RemoveAllMessages(userId);
                if (a==false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRemoveMessage, CustomErrorMessage.RemoveErrorMessage, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.RemoveMessage,a));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRemoveMessage, ex.Message, ""));
            }
        }

        [HttpPost("RemoveMessageById")]
        public async Task<ActionResult> RemoveMessageById(long messageId)
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateMessageId(messageId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRemoveMessage, ""));
                }
                var a = await _chatSevice.RemoveMessagesById(userId,messageId);
                if (a == false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRemoveMessage, CustomErrorMessage.RemoveErrorMessage, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.RemoveMessage, a));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRemoveMessage, ex.Message, ""));
            }
        }
    }
}
