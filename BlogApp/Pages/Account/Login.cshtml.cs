using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using BlogApp.Models;

namespace BlogApp.Pages.Account
{
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
