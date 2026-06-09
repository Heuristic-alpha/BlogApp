using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Pages.Account
{
    [EnableRateLimiting(Constants.RateLimiterNames.PublicFixLimit)]
    public class SignUpModel : PageModel
    {
        private AppUserManager _appUserManager;

        public SignUpModel(AppUserManager appUserManager)
        {
            _appUserManager = appUserManager;
        }

        [BindProperty, Required]
        public string UserName { get; set; } = string.Empty;

        [BindProperty, Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [BindProperty, Required]
        public string Password { get; set; } = string.Empty;

        [BindProperty, Required]
        public string ConfirmPassword { get; set; } = string.Empty;

        [BindProperty]
        public bool IsMale { get; set; } = true;

        public void OnGet()
        {

        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                if (Password != ConfirmPassword)
                {
                    ModelState.AddModelError(string.Empty, "Password and ConfirmPassword should be equal");
                    return Page();
                }
                (bool isSuccess, List<string> errors) result = await _appUserManager.TryCreateUserAsync(UserName, Email, Password, IsMale, ["Members"]);
                if (result.isSuccess)
                {
                    return RedirectToPage("CreatedPage");
                }
                else
                {
                    foreach (string err in result.errors)
                    {
                        ModelState.AddModelError("", err);
                    }
                    return Page();
                }
            }
            return Page();
        }

        public string GetIconPath(bool isMale)
        {
            if (isMale) return Url.Content(Constants.StaticImagesURL.MaleUserProfileIcon);
            else return Url.Content(Constants.StaticImagesURL.FemaleUserProfileIcon);
        }
    }
}
