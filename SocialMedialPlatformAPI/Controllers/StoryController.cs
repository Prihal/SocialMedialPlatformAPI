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
    public class StoryController : ControllerBase
    {
        private readonly IStoryServicecs _storyServicecs;
        private readonly Helper _helper;
        private readonly IValidationService _validationService;
        private readonly ResponseHandler _responseHandler;

        public StoryController(IStoryServicecs storyServicecs,Helper helper,IValidationService validationService,ResponseHandler responseHandler)
        {
            _storyServicecs = storyServicecs;
            _helper = helper;
            _validationService = validationService;
            _responseHandler = responseHandler;

          
        }

        [HttpPost("AddStory")]
        public async Task<ActionResult> AddStory(StoryDto storyDto)
        {
            try
            {
            long userId = _helper.GetUserIdClaim();
            List<ValidationError> errors = _validationService.ValidateUserStoryData(userId,storyDto);
            if (errors.Any())
            {
              return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStoryAdd, ""));
            }
               var a=await _storyServicecs.AddStory(userId,storyDto);
                if (a == false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryError, CustomErrorMessage.StoryUploadError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.UploadStory,a));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryError, ex.Message, ""));
            }
        }

        [HttpPost("DeleteStory")]
        public async Task<ActionResult> DeleteStory(long storyId)
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateStoryId(storyId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStoryDelete, ""));
                }
                var a=await _storyServicecs.DeleteStory(userId,storyId);
                if (a == false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryDeleteError, CustomErrorMessage.StoryDeleteError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.DeleteStory, a));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryDeleteError, ex.Message, ""));
            }
        }

        [HttpPost("SetHighlight")]
        public async Task<ActionResult> SetHighlight(long storyId)
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateStoryId(storyId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, ""));
                }
                var a = await _storyServicecs.SetHighlight(userId, storyId);
                if (a == false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryError, CustomErrorMessage.Highlight, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.SetHighlight, a));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryError, ex.Message, ""));
            }
        }

        [HttpPost("RemoveHighlight")]
        public async Task<ActionResult> RemoveHighlight(long storyId)
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateStoryId(storyId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, ""));
                }
                var a = await _storyServicecs.RemoveHighlight(userId, storyId);
                if (a == false)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryError, CustomErrorMessage.RemoveHighlight, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.RemoveHighlights, a));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryError, ex.Message, ""));
            }
        }

        [HttpGet("GetStory")]
        public async Task<ActionResult> GetStory()
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateUserId(userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, ""));
                }
                PaginationResponseModel<GetStoryDto> getStoryDto = await _storyServicecs.GetStory(userId);
                if (getStoryDto == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.GetStoryError, CustomErrorMessage.GetStoryErrors, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetStory, getStoryDto));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryError, ex.Message, ""));
            }
        }

        [HttpGet("GetAllStory")]
        public async Task<ActionResult> GetAllStory()
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateUserId(userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationStory, ""));
                }
                PaginationResponseModel<GetAllStoryDto> getStoryDto =await _storyServicecs.GetAllStory(userId);
                if (getStoryDto == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.GetStoryError, CustomErrorMessage.GetStoryErrors, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetStory, getStoryDto));
            }
            catch (SqlException exp)
            {
                throw new Exception("SqlException While Login" + exp.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.StoryError, ex.Message, ""));
            }
        }
    }
}
