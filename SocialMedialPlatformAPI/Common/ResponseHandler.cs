using SocialMedialPlatformAPI.DTO;

namespace SocialMedialPlatformAPI.Common
{
    public class ResponseHandler
    {
        //Return 200 Ok
        public ResponseModel Success(string Message,Object Data)
        {
            return new ResponseModel
            {
                IsSuccess = true,
                Message = Message,
                Data = Data,
                StatusCode = StatusCodes.Status200OK
            };
        }

        //Response #400 BadRequest
        public ResponseModel BadRequest(string ErrorCode, string Message, Object Data)
        {
            return new ResponseModel
            {
                IsSuccess = false,
                Message = Message,
                Data = Data,
                StatusCode = StatusCodes.Status400BadRequest,
                ErrorCode = ErrorCode
            };
        }

        //Response #404 NotFoundRequest
        public ResponseModel NotFoundRequest(string ErrorCode, string Message, Object Data)
        {
            return new ResponseModel
            {
                IsSuccess = false,
                Message = Message,
                Data = Data,
                StatusCode = StatusCodes.Status404NotFound,
                ErrorCode = ErrorCode
            };
        }
    }
}
