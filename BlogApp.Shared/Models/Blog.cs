using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using BlogApp.Models.JoinModels;

namespace BlogApp.Models
{
    public class Blog
    {
        public long BlogId { get; set; }

        [Required(ErrorMessage = "cant be empty")]
        [StringLength(30, MinimumLength = 3, ErrorMessage = "should be between 3 to 30 chars")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "cant be empty")]
        [StringLength(5000, MinimumLength = 10, ErrorMessage = "should be between 10 to 5000 chars")]
        public string BlogContent { get; set; } = string.Empty;

        public bool IsConfirmed { get; set; } = false;
        public bool IsPublic { get; set; } = false;

        public DateTime CreationDateTime { get; set; } = DateTime.Now;

        /// <summary>
        /// A nav link to user that own this Blog
        /// </summary>
        public long? AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }

        /// <summary>
        /// A nav link to its comments
        /// </summary>
        public virtual IEnumerable<Comment>? Comments { get; set; }

        /// <summary>
        /// A nav link to its categories
        /// </summary>
        public virtual IEnumerable<BlogCategory>? BlogCategories { get; set; }
    }
}
