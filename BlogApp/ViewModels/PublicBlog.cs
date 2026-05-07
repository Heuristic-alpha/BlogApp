namespace BlogApp.ViewModels
{
    public class PublicBlog
    {
        public long BlogId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public DateTime PublishDate { get; set; }
        public string[] Categories { get; set; } = Array.Empty<string>();      

    }
}
