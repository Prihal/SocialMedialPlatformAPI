namespace SocialMedialPlatformAPI.DTO;

public class PaginationResponseModel<T> where T : class
{
    public int TotalRecord {  get; set; }
    public int PageSize {  get; set; }

    public int PageNumber {  get; set; }
    public int RequiredPage {  get; set; }

    public required List<T> Records { get; set; }
}
