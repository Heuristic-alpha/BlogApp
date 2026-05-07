namespace BlogApp.Models.SearchModels
{
    public class CommentSearch : SearchModelBase
    {
        public long? CommentId { get; set; }
        public string? CommentText { get; set; }
        public bool? IsConfirmed { get; set; }
        public long? AppUserId { get; set; }
        public long? BlogId { get; set; }
    }
}
