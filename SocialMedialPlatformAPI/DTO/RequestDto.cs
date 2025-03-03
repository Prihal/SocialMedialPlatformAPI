namespace SocialMedialPlatformAPI.DTO;

public class RequestDto<T> where T : class
{
    public int PageNumber {  get; set; }
    public int PageSize { get; set; }
    public string? SearchName {  get; set; }
    public required T Model { get; set; }
}
