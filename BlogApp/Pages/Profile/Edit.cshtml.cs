using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Pages.Profile
{
    [Authorize(Roles = $"{Constants.Roles.Members}")]
    public class EditModel : PageModel
    {
        // 250 KB
        const int MAX_File_Length = 262144;

        static string[] s_ValidExtension = new string[] { ".jpeg", ".png", ".jpg" };

        private DataDbContext _dataDbContext;
        private UserManager<IdentityAppUser> _userManager;
        private UserProfilePictureService _userProfilePictureService;
        private ILogger<EditModel> _logger;

        public EditModel(DataDbContext dataDbContext, UserManager<IdentityAppUser> userManager, ILogger<EditModel> logger, UserProfilePictureService userProfilePictureService)
        {
            _dataDbContext = dataDbContext;
            _userManager = userManager;
            _logger = logger;
            _userProfilePictureService = userProfilePictureService;
        }

        [BindProperty]
        public long UserId { get; set; }

        [BindProperty, Required]
        public string UserName { get; set; } = string.Empty;

        [BindProperty, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        public string Description { get; set; } = string.Empty;

        [BindProperty]
        public IFormFile ProfilePicture { get; set; }

        [BindProperty]
        public bool ShouldChangeProfilePicture { get; set; }

        [BindProperty]
        public bool ShouldDeleteProfilePicture { get; set; }

        public string ProfilePictureUrl { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            AppUser? appUser = HttpContext.GetAppUser();
            if (appUser == null) return NotFound();

            UserId = appUser.AppUserId;
            UserName = identityAppUser.UserName ?? "DEBUG: null";
            Email = identityAppUser.Email ?? "DEBUG: null";
            Description = appUser.Description;
            ProfilePictureUrl = await _userProfilePictureService.GetProfilePictureURLAsync(appUser.AppUserId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null || identityAppUser.AppUserId != UserId) return RedirectToPage("/Account/AccessDenied");

            // skip validation state of profile picture if it should not changed
            if (!ShouldChangeProfilePicture || ShouldDeleteProfilePicture)
            {
                string modelName = nameof(ProfilePicture);
                ModelState.ClearValidationState(modelName);
                ModelState.MarkFieldValid(modelName);
            }

            if (ModelState.IsValid)
            {
                // 1- change user description:
                AppUser appUser = HttpContext.GetAppUser()!;
                appUser.Description = Description;
                _dataDbContext.Update(appUser);

                // 2- change user name:
                if (!UserName.Equals(identityAppUser.UserName, StringComparison.InvariantCultureIgnoreCase))
                {
                    IdentityAppUser? identity = await _userManager.GetUserAsync(HttpContext.User);
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

                // 4- change user profile picture
                if (ShouldChangeProfilePicture)
                {
                    if (ShouldDeleteProfilePicture)
                    {
                        await _userProfilePictureService.RemoveProfilePictureAsync(identityAppUser.AppUserId);
                    }
                    else
                    {
                        // check picture size:
                        if (ProfilePicture.Length > MAX_File_Length)
                        {
                            ModelState.AddModelError(nameof(ProfilePicture), "File lether than 250KB accepted");
                            return Page();
                        }

                        // check picture extension:
                        string fileExtension = Path.GetExtension(ProfilePicture.FileName);
                        if (!s_ValidExtension.Any(ve => ve.Equals(fileExtension)))
                        {
                            ModelState.AddModelError(nameof(ProfilePicture), $"Only picture with [{string.Join("  ", s_ValidExtension)}] formats is accepted");
                            return Page();
                        }

                        await _userProfilePictureService.SavePictureAsync(ProfilePicture, identityAppUser.AppUserId);
                    }                
                }

                await _dataDbContext.SaveChangesAsync();
                return RedirectToPage("/Dashboard");
            }
            return Page();
        }
    }
}
