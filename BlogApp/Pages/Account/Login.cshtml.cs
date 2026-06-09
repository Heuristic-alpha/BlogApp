using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Pages.Account
{
    [EnableRateLimiting(Constants.RateLimiterNames.PublicFixLimit)]
    public class LoginModel : PageModel
    {
        public LoginModel(SignInManager<IdentityAppUser> signInManager)
        {
            SignInManager = signInManager;
        }

        public SignInManager<IdentityAppUser> SignInManager { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        [BindProperty, Required]
        public string UserName { get; set; } = string.Empty;

        [BindProperty, Required]
        public string Password { get; set; } = string.Empty;


        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var result = await SignInManager.PasswordSignInAsync(UserName, Password, false, false);              
                if (result.Succeeded)
                {
                    return Redirect(ReturnUrl ?? "/");
                }
                ModelState.AddModelError("", "Invalid username or password");
            }
            return Page();
        }
    }
}
