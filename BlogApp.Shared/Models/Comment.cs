using BlogApp.Models.JoinModels;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Comment
    {
        public long CommentId { get; set; }

        [Required]
        [StringLength(2000, MinimumLength = 10, ErrorMessage = "should be between 10 to 2000 chars")]
        public string StringContent { get; set; } = string.Empty;

        public bool IsConfirmed { get; set; }

        public int Like => AppUserCommnets?.Count(cv => cv.VoteType == Enums.VoteType.Like) ?? -1;
        public int Dislike => AppUserCommnets?.Count(cv => cv.VoteType == Enums.VoteType.Dislike) ?? -1;

        public long? AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }

        public long? BlogId { get; set; }
        public virtual Blog? Blog { get; set; }

        public DateTime CreationDateTime { get; set; } = DateTime.Now;

        public virtual IEnumerable<AppUserCommnet>? AppUserCommnets { get; set; }
    }
}
