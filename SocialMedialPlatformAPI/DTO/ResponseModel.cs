namespace SocialMedialPlatformAPI.DTO;

public class ResponseModel
{
    public bool IsSuccess {  get; set; }

    public int StatusCode { get; set; }

    public string? Message { get; set; }

    public object? Data { get; set; }

    public string? ErrorCode {  get; set; }
}
