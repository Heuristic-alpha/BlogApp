namespace BlogApp.Models.SearchModels
{
    public class BlogSearch : SearchModelBase
    {
        public long? BlogId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public bool? IsConfirmed { get; set; }
        public bool? IsPublic { get; set; }

        public long? AppUserId { get; set; }
    }
}
