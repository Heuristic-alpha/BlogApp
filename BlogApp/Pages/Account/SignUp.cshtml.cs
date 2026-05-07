using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using BlogApp.Infrastructures;
using BlogApp.Models;

namespace BlogApp.Pages.Account
{
    public class SignUpModel : PageModel
    {
        public UserManager<IdentityAppUser> UserManager { get; set; }
        public DataDbContext DbContext { get; set; }

        public SignUpModel(UserManager<IdentityAppUser> userManager, DataDbContext dbContext)
        {
            UserManager = userManager;
            DbContext = dbContext;
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
                (bool isSuccess, List<string> errors) result = await AppUserHelper.TryCreateUserAsync(UserManager, DbContext, UserName, Email, Password, IsMale, ["Members"]);
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
            if (isMale) return Url.Content("images/static/icon-male.svg");
            else return Url.Content("images/static/icon-female.svg");
        }
    }
}
