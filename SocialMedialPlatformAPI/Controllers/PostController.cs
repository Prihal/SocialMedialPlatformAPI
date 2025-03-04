using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
    public class PostController : ControllerBase
    {
        private readonly IValidationService _validationService;
        private readonly ResponseHandler _responseHandler;
        private readonly Helper _helper;
        private readonly IPostService _postService;

        public PostController(IValidationService validationService,ResponseHandler responseHandler,Helper helper,IPostService postService)
        {
            _validationService = validationService;
            _responseHandler = responseHandler;
            _helper = helper;
            _postService = postService;
        }

        [HttpPost("CreatePost")]
        public async Task<ActionResult> CreatePost([FromForm]CreatePostDto createPostDto)
        {
            //insert database for media type
            //INSERT INTO dbo.MediaType(MediaTypeId, MediaType) VALUES(1, 'Image');
            //INSERT INTO dbo.MediaType(MediaTypeId, MediaType) VALUES(2, 'Video');
            //INSERT INTO dbo.MediaType(MediaTypeId, MediaType) VALUES(3, 'Reel');
            //INSERT INTO dbo.MediaType(MediaTypeId, MediaType) VALUES(4, 'Post');

            try
            {
                List<ValidationError> errors = _validationService.ValidateCreatePost(createPostDto);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                PostResponseDTO responseDTO = await _postService.CreatePost(createPostDto);
                if (responseDTO == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPost, CustomErrorMessage.PostError, ""));
                }
                if (createPostDto.PostId <= 0)
                {
                    return Ok(_responseHandler.Success(CustomErrorMessage.CreatePost, responseDTO));
                }
                else
                {
                    return Ok(_responseHandler.Success(CustomErrorMessage.UpdatePost, responseDTO));
                }
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPost, ex.Message, ""));
                }
            }
        }

        [HttpPost("GetPostById")]
        public async Task<ActionResult> GetPostById([FromBody]long postId,string postType)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetPostById(postId,postType);
                if(errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetPost,CustomErrorMessage.PostGetError,errors));
                }
                PostResponseDTO postResponseDto = await _postService.GetPostById(postId,postType);
                if(postResponseDto == null)
                {

                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetPost,CustomErrorMessage.PostGetError,""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetPost,postResponseDto));
            }
            catch (Exception ex)
            { 
                if(ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode,vx.Message,vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetPost,ex.Message,""));
                }
            }
        }

        [HttpPost("Post&ReelListById")]
        public async Task<ActionResult> GetPostAndReelListById([FromBody]RequestDto<PostListRequestDto> requestDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidatePostList(requestDto);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                PaginationResponseModel<PostResponseDTO> postResponseDto = await _postService.GetPostAndReelListById(requestDto);
                if (postResponseDto == null)
                {

                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList, CustomErrorMessage.GetList, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetPost, postResponseDto));
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

        [HttpPost("GetPostListByUserId")]
        public async Task<ActionResult> GetPostListByUserIdAsync([FromBody] PaginationRequestDto model)
        
        {
            try
            {
                PaginationResponseModel<PostResponseDTO> data = await _postService.GetPostListById(model);
                if (data == null)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsGetList, CustomErrorMessage.GetFollowerList, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.GetListSuccess, data));
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

        [HttpPost("DeletePost")]
        public async Task<ActionResult> DeletePostById(long postId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateGetPostId(postId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool isDeleted = await _postService.DetelePostById(postId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, CustomErrorMessage.PostDeleteError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.PostDelete, postId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, ex.Message, ""));
                }
            }
        }

        [HttpPost("Like&UnLikePost")]
        public async Task<ActionResult> LikeAndUnlikePost(LikePostDto likePostDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateLikePost(likePostDto.UserId, likePostDto.PostId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool IsLike = await _postService.LikeAndUnlikePost(likePostDto);
                if (!IsLike)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostLike, CustomErrorMessage.PostLikeError, ""));
                }
                if(IsLike==true)
                {
                    return Ok(_responseHandler.Success(CustomErrorMessage.PostLike, likePostDto.PostId));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.PostUnLike, likePostDto.PostId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostLike, ex.Message, ""));
                }
            }
        }

        [HttpPost("CommentPost")]
        public async Task<ActionResult> CommentPost(CommentPostDto commentPostDto)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateCommentPost(commentPostDto);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool isComment = await _postService.CommentPost(commentPostDto);

                return Ok(_responseHandler.Success(CustomErrorMessage.PostComment, commentPostDto));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostComment, ex.Message, ""));
                }
            }
        }

        [HttpPost("DeletePostComment")]
        [Authorize]
        public async Task<ActionResult> DetelePostComment(long commentId)
        {
            try
            {
                List<ValidationError> errors = _validationService.ValidateCommentId(commentId);
                if (errors.Any())
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsValid, CustomErrorMessage.ValidationPost, errors));
                }
                bool isDeleted = await _postService.DetelePostComment(commentId);
                if (!isDeleted)
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostComment, CustomErrorMessage.PostCommentError, ""));
                }
                return Ok(_responseHandler.Success(CustomErrorMessage.PostCommentDelete, commentId));
            }
            catch (Exception ex)
            {
                if (ex is ValidationException vx)
                {
                    return BadRequest(_responseHandler.BadRequest(vx.ErrorCode, vx.Message, vx.Errors));
                }
                else
                {
                    return BadRequest(_responseHandler.BadRequest(CustomErrorCode.IsPostDelete, ex.Message, ""));
                }
            }
        }
    }
}
