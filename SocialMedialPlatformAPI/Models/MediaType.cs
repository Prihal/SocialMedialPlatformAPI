namespace SocialMedialPlatformAPI.Models
{
    public partial class MediaType
    {
        public int MediaTypeId { get; set; }

        public string MediaType1 { get; set; } = null!;

        public virtual ICollection<PostMapping> PostMappings { get; set; } = new List<PostMapping>();
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();

        public virtual ICollection<Story> Stories { get; set; } = new List<Story>();
    }

}
