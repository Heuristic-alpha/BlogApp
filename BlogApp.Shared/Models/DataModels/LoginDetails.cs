using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.DataModels
{
    public class LoginDetails
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
