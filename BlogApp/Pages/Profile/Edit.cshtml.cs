using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace BlogApp.Pages.Profile
{
    [Authorize(Roles = $"{Constants.Roles.Members}")]
    public class EditModel : PageModel
    {
        private DataDbContext _dataDbContext;
        private UserManager<IdentityAppUser> _userManager;
        private ILogger<EditModel> _logger;

        public EditModel(DataDbContext dataDbContext, UserManager<IdentityAppUser> userManager, ILogger<EditModel> logger)
        {
            _dataDbContext = dataDbContext;
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public long UserId { get; set; }

        [BindProperty, Required]
        public string UserName { get; set; }

        [BindProperty, EmailAddress]
        public string Email { get; set; }

        [BindProperty]
        public string Description { get; set; }

        [BindProperty]
        public IFormFile FormFile { get; set; }

        public IActionResult OnGet()
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            AppUser? appUser = HttpContext.GetAppUser();
            if (appUser == null) return NotFound();

            UserId = appUser.AppUserId;
            UserName = identityAppUser.UserName ?? "DEBUG: null";
            Email = identityAppUser.Email ?? "DEBUG: null";
            Description = appUser.Description;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null || identityAppUser.AppUserId != UserId) return RedirectToPage("/Account/AccessDenied");

            if (ModelState.IsValid)
            {
                // 1- change user description:
                AppUser appUser = HttpContext.GetAppUser()!;
                appUser.Description = Description;
                _dataDbContext.Update(appUser);

                // 2- change user name:
                if (!UserName.Equals(identityAppUser.UserName, StringComparison.InvariantCultureIgnoreCase))
                {
                    IdentityAppUser? identity =await _userManager.GetUserAsync(HttpContext.User);
                    var identityChangeResult = await _userManager.SetUserNameAsync(identity!, UserName);
                    if (!identityChangeResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, string.Join("\n", identityChangeResult.Errors.Select(e => e.Description)));
                        return Page();
                    }
                    appUser.DisplayName = UserName;
                    _dataDbContext.Update(appUser);
                }

                // 3- change user email:
                if (!Email.Equals(identityAppUser.Email, StringComparison.InvariantCultureIgnoreCase))
                {
                    IdentityAppUser? identity = await _userManager.GetUserAsync(HttpContext.User);
                    var identityChangeResult = await _userManager.SetEmailAsync(identity!, Email);
                    if (!identityChangeResult.Succeeded)
                    {
                        ModelState.AddModelError(string.Empty, string.Join("\n", identityChangeResult.Errors.Select(e => e.Description)));
                        return Page();
                    }
                }

                // 4- change user prifile picture
                // debug:
                StringBuilder sb = new StringBuilder();
                sb.AppendLine($"Uploaded file from user is:");
                sb.AppendLine($"Name: {FormFile.Name}");
                sb.AppendLine($"FileName: {FormFile.FileName}");
                sb.AppendLine($"ContentType: {FormFile.ContentType}");
                sb.AppendLine($"Length: {FormFile.Length}");
                _logger.LogWarning( sb.ToString() );

                await _dataDbContext.SaveChangesAsync();
                return RedirectToPage("/Dashboard");
            }
            return Page();          
        }
    }
}
