
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedialPlatformAPI.Common;
using SocialMedialPlatformAPI.Data;
using SocialMedialPlatformAPI.DTO;
using SocialMedialPlatformAPI.Helpers;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Utils;

namespace SocialMedialPlatformAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext context;
        private readonly IValidationService _validationService;
        private readonly ResponseHandler _responseHandler;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly Helper _helper;

        public UserController(AppDbContext context,IValidationService validationService,ResponseHandler responseHandler,IUserService userService,IAuthService authService,Helper helper)
        {
            this.context = context;
            _validationService = validationService;
            _responseHandler = responseHandler;
            _userService = userService;
            _authService = authService;
            _helper = helper;
        }
        [HttpPost("UploadProfilePhoto")]
        public async Task<ActionResult> UploadProfilePhoto(IFormFile ProfilePhoto)
        {
            try
            {
                long userId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateProfileFile(ProfilePhoto, userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationProfile, errors));
                }

                ProfilePhotoResponseDto profilePhotoDTO = await _userService.UploadProfilePhoto(ProfilePhoto, userId);
                if (profilePhotoDTO == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, CustomErrorMessage.UploadError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.UploadPhoto, profilePhotoDTO));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, ""));
                }
            }
        }

        [HttpPost("UpdateProfile")]
        public async Task<ActionResult> UpdateProfile(UserDto userDto)
        {
            try
            {
                userDto.UserId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateRegistration(userDto);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationUpdateProfile, errors));
                }

                UserDto? user = await _authService.InsertUserData(userDto);
                if (user == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpdate, CustomErrorMessage.UpdateProfile, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.UpdateProfileSuccess, user));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsUpload, ex.Message, ""));
                }
            }
        }

        [HttpPost("FollowRequest")]
        public async Task<ActionResult> FollowRequest(FollowRequestDto followRequestDto)
        {
            try
            {
                long fromuserId = _helper.GetUserIdClaim();
                List<ValidationError> errors = _validationService.ValidateFollowRequest(followRequestDto,fromuserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationUpdateProfile, errors));
                }
                bool isFollow = await _userService.FollowRequest(followRequestDto, fromuserId);
                if (!isFollow)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRequest, CustomErrorMessage.RequestError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.RequestSuccess, followRequestDto));

            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsRequest, ex.Message, ""));
                }
            }

        }

        [HttpPost("FollowerOrFollowingListById")]
        public async Task<ActionResult> FollowerOrFollowingListById([FromBody]RequestDto<FollowerListRequestDto> requestDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateFollowerOrFollowingList(requestDto);
                if(errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid,CustomErrorMessage.RequestError,errors));
                }
                PaginationResponseModel<UserDto> data =await _userService.FollowerOrFollowingListById(requestDto);
                if(data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList,CustomErrorMessage.GetFollowerList,""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetFollowerListSuccess,data));


            }
            catch (Exception ex) 
            { 
                if(ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode,vx.Message,vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList,ex.Message,""));
                }
            }
        }

        [HttpPost("GetUserListByUserName")]
        public async Task<ActionResult> GetUserListByUserName([FromBody]RequestDto<UserIdRequestDto> requestDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateUserId(requestDto.Model.UserId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationList, errors));
                }
                PaginationResponseModel<UserDto> data = await _userService.GetUserListByUserName(requestDto);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList, CustomErrorMessage.GetFollowerList, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetFollowerListSuccess, data));
            }
            catch (Exception ex) {
                if(ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode,vx.Message,vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList, ex.Message, ""));
                }
            }
        }

        [HttpGet("GetUserById")]
        public async Task<ActionResult> GetUserById(long UserId)
        {
            try
            {
                List<ValidationError> errors= _validationService.ValidateUserId(UserId);
                if (errors.Any()) 
                { 
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid,CustomErrorMessage.ExistUser,errors));
                }
                UserDto data = await _userService.GetUserData(UserId);
                if(data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsNotExist,CustomErrorMessage.ExistUser,""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetUser,data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList, ex.Message, ""));
                }
            }
        }

        [HttpPost("RequestAcceptOrCancle")]
        public async Task<ActionResult> RequestAcceptOrCancle([FromQuery]long requestId,string acceptType)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRequestAccept(requestId,acceptType);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReqType, errors));
                }
                bool isAccept = await _userService.RequestAcceptOrCancel(requestId, acceptType);
                if (!isAccept)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationReqType, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.AccpteUpdate, requestId));
            }
            catch(Exception ex)
            {
                if(ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode,vx.Message,vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList,ex.Message,""));
                }
            }
        }

        [HttpPost("RequestListById")]
        public async Task<ActionResult> RequestListById([FromBody]RequestDto<FollowRequestDto> requestDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateRequestList(requestDto);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationRequest, errors));
                }
                PaginationResponseModel<RequestListResponseDto> data = await _userService.GetRequestListById(requestDto);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList, CustomErrorMessage.GetFollowerList, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetFollowerListSuccess, data));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList, ex.Message, ""));
                }
            }
        }

        [HttpGet("Follower,Following,PostCountById{userId}")]
        public async Task<ActionResult> FollowerFollowingPostCountById(long userId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateUserId(userId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExistUser, errors));
                }
                CountResponseDto count = await _userService.GetFollowerFollowingPostCountById(userId);
                if (count == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsCount, CustomErrorMessage.CountError, ""));
                }

                return Ok(_responseHandler.Success(CustomErrorMessage.CountSucces, count));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList, ex.Message, ""));
                }
            }
        }
    }
}
