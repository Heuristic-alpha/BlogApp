using BlogApp.Models.Enums;

namespace BlogApp.Models.DataModels
{
    public class CommentVoteDto
    {
        public long CommentId { get; set; }
        public long AppUserId { get; set; }
        public VoteType VoteType { get; set; }
        public string ReturnUrl { get; set; } = string.Empty;
    }
}
