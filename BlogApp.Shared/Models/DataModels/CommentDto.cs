using BlogApp.Models.Enums;

namespace BlogApp.Models.DataModels
{
    public class CommentDto
    {
        public long BlogId { get; set; } // to Blog
        public long AppUserId { get; set; } // from User
        public string Text { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"<(BlogId={BlogId}), (AppUserId={AppUserId}), (Text={Text}), (ReturnUrl={ReturnUrl})>";
        }
    }
}
