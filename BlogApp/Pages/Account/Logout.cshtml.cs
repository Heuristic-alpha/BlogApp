using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using System.ComponentModel.DataAnnotations;

namespace BlogApp.Pages.Account
{
    [EnableRateLimiting(Constants.RateLimiterNames.PublicFixLimit)]
    public class LogoutModel : PageModel
    {
        public LogoutModel(SignInManager<IdentityAppUser> signInManager)
        {
            SignInManager = signInManager;
        }

        public SignInManager<IdentityAppUser> SignInManager { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            await SignInManager.SignOutAsync();
            await Task.Delay(1000); // Fun
            return RedirectToAction("Index","Home");
        }
    }
}
