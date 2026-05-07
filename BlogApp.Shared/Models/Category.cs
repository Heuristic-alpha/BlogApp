using BlogApp.Models.JoinModels;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models
{
    public class Category
    {
        public long CategoryId { get; set; }

        [Required]
        [StringLength(15,MinimumLength = 3,ErrorMessage = "Name should be between 3 to 15 charcters")]
        public string Name { get; set; } = string.Empty;

        public virtual IEnumerable<BlogCategory>? BlogCategories { get; set; }
    }
}
