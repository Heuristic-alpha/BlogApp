using System.ComponentModel.DataAnnotations;
using BlogApp.Models;
using BlogApp.Models.Enums;

namespace BlogApp.Models.JoinModels
{
    public class AppUserCommnet
    {
        public long AppUserCommnetId { get; set; }
        public VoteType VoteType { get; set; } = VoteType.Like;

        public long AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }

        public long CommentId { get; set; }
        public virtual Comment? Comment { get; set; }
    }
}
