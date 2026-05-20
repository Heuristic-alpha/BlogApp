using BlogApp.Models.JoinModels;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BlogApp.Models
{
    public class AppUser
    {
        public long AppUserId { get; set; }

        [Required(ErrorMessage = "DisplayName cant be empty")]
        [StringLength(20, MinimumLength = 4, ErrorMessage = "DisplayName length should be 4 to 20 chars")]
        public string DisplayName { get; set; } = string.Empty;

        public bool IsMale { get; set; } = true;

        [StringLength(2000, MinimumLength = 0, ErrorMessage = "Description length should be 0 to 2000 chars")]
        public string Description { get; set; } = string.Empty;

        public DateTime CreationDateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// Navigation property to Blogs
        /// </summary>
        public virtual IEnumerable<Blog>? Blogs { get; set; }

        /// <summary>
        /// Navigation property to Comments
        /// </summary>
        public virtual IEnumerable<Comment>? Comments { get; set; }

        /// <summary>
        /// Navigation property to CommentVotes (join table)
        /// </summary>
        public virtual IEnumerable<AppUserCommnet>? AppUserCommnets { get; set; }

        /// <summary>
        /// Navigation property to Blog vote and comment (join table)
        /// </summary>
        public virtual IEnumerable<AppUserBlog>? AppUserBlogs { get; set; }

        /// <summary>
        /// Navigation property to AppUserOptional (one to one relationship)
        /// </summary>
        public virtual AppUserOptional? AppUserOptional { get; set; }

        /// <summary>
        /// link to IdentityAppUser that is in IdentityContext Database
        /// </summary>
        public string? AppIdentityId { get; set; }
    }
}
