using BlogApp.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp.Models.JoinModels
{
    /// <summary>
    /// This type is interaction of AppUser with Blog. it contains blog voteType and optional FK to AppUser Comment, because every AppUser can only have one Comment on any blogs.
    /// </summary>
    public class AppUserBlog
    {
        public long AppUserBlogId { get; set; }

        public VoteType VoteType { get; set; } = VoteType.Dislike;
        public long? CommentId { get; set; }

        public long AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }

        public long BlogId { get; set; }
        public virtual Blog? Blog { get; set; }
    }
}
