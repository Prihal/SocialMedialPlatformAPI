using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SocialMedialPlatformAPI.Common;
using SocialMedialPlatformAPI.Interface;
using SocialMedialPlatformAPI.Utils;

namespace SocialMedialPlatformAPI.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FileController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly ResponseHandler _responseHandler;
        private readonly IUserService _userService;

        public FileController(IValidationService validationService,ResponseHandler responseHandler,IUserService userService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _userService = userService;
        }
        [HttpGet("ProfilePhoto")]
        public async Task<ActionResult> GetProfilePhoto(long userId,string imageName)
        {
            List<ValidationError> errors = _validationService.ValidateUserId(userId);
            if (errors.Any())
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExistUser, errors));
            }

            int index = imageName.IndexOf('.') + 1;
            string extension = imageName[index..];
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId.ToString(), "ProfilePhoto", imageName);
            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPath, CustomErrorMessage.PathNotExits, imageName));
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            string fileType = _userService.GetContentType(extension);

            return Ok(_responseHandler.Success(CustomErrorMessage.GetSuccess, new { ImageBase64 = base64String, FileType = fileType }));
        }

        [HttpGet("GetPost")]
        public async Task<ActionResult> GetPost(long UserId,string postName)
        {
            List<ValidationError> errors = _validationService.ValidateUserId(UserId);
            if (errors.Any())
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExistUser, errors));
            }

            int index = postName.IndexOf('.') + 1;
            string extension = postName[index..];
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", UserId.ToString(), "Post", postName);
            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPath, CustomErrorMessage.PathNotExits, postName));
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            string fileType = _userService.GetContentType(extension);

            return Ok(_responseHandler.Success(CustomErrorMessage.GetSuccess, new { ImageBase64 = base64String, FileType = fileType }));
        }

        [HttpGet("Reel")]
        public IActionResult GetReel(long userId, string reelName)
        {
            List<ValidationError> errors = _validationService.ValidateUserId(userId);
            if (errors.Any())
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ExistUser, errors));
            }

            int index = reelName.IndexOf('.') + 1;
            string extension = reelName[index..];
            string imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "content", "User", userId.ToString(), "Reel", reelName);
            if (!System.IO.File.Exists(imagePath))
            {
                return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPath, CustomErrorMessage.PathNotExits, reelName));
            }

            byte[] imageBytes = System.IO.File.ReadAllBytes(imagePath);
            string base64String = Convert.ToBase64String(imageBytes);
            string fileType = _userService.GetContentType(extension);

            return Ok(_responseHandler.Success(CustomErrorMessage.GetSuccess, new { ImageBase64 = base64String, FileType = fileType }));
        }
    }
}
