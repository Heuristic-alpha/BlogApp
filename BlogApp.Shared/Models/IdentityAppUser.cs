using Microsoft.AspNetCore.Identity;

namespace BlogApp.Models
{
    public class IdentityAppUser : IdentityUser
    {
        public long AppUserId { get; set; }
    }
}
