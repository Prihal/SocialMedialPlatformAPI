using SocialMedialPlatformAPI.DTO;

namespace SocialMedialPlatformAPI.Interface
{
    public interface IPostService
    {
        Task<PostResponseDTO> CreatePost(CreatePostDto createPostDto);
        Task<PostResponseDTO> GetPostById(long postId, string postType);
        Task<PaginationResponseModel<PostResponseDTO>> GetPostAndReelListById(RequestDto<PostListRequestDto> requestDto);
        Task<PaginationResponseModel<PostResponseDTO>> GetPostListById(PaginationRequestDto paginationRequestDto);

        Task<bool> DetelePostById(long postId);
        Task<bool> LikeAndUnlikePost(LikePostDto likePostDto);
        Task<bool> CommentPost(CommentPostDto commentPostDto);
        Task<bool> DetelePostComment(long commentId);


    }
}
