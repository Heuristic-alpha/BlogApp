using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogApp.Models
{
    /// <summary>
    /// This model contain optional property of AppUser model that are not necessary.
    /// <remark>
    /// AppUserOptional Has 1 to 1 relationship with AppUser model.
    /// </remark>
    /// </summary>
    public class AppUserOptional
    {
        public long AppUserOptionalId { get; set; }

        public string ProfilePictureURL { get; set; } = string.Empty;

        public long AppUserId { get; set; }
        public virtual AppUser? AppUser { get; set; }
    }
}
