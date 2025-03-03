namespace SocialMedialPlatformAPI.DTO;

public class ResponseMessageDto
{
    public long FromUserId {  get; set; }
    public List<GetMessageList> MessageList {  get; set; }
}

public class GetMessageList
{
    public string Message { get; set; }
}
