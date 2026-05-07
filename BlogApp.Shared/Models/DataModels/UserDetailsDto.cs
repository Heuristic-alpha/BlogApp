using System.ComponentModel.DataAnnotations;

namespace BlogApp.Models.DataModels
{
    public class UserDetailsDto
    {
        public long AppUserId { get; set; }
        public bool IsMale { get; set; } = true;
        public string? Description { get; set; } = string.Empty;
        public DateTime CreationDateTime { get; set; }
        public string IdentityAppUserId { get; set; } = string.Empty;
        [StringLength(20, MinimumLength = 4, ErrorMessage = "DisplayName length should be 4 to 20 chars")]
        public string UserName { get; set; } = string.Empty;
        [EmailAddress] public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public Dictionary<string, bool>? RolesDict { get; set; }

        public IdentityAppUser? IdentityAppUser { get; set; }

        /// <summary>
        /// Convert to AppUser Model
        /// </summary>
        public AppUser ToAppUser()
        {
            return new AppUser()
            {
                AppUserId = AppUserId,
                AppIdentityId = IdentityAppUserId,
                DisplayName = UserName,
                Description = Description ?? string.Empty,
                IsMale = IsMale,
                CreationDateTime = CreationDateTime,
            };
        }
    }
}
