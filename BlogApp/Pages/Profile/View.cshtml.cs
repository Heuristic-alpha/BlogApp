using BlogApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;

namespace BlogApp.Pages.Profile
{
    [EnableRateLimiting(Constants.RateLimiterNames.PublicFixLimit)]
    public class ViewModel : PageModel
    {
        private DataDbContext _dataDbContext;

        public ViewModel(DataDbContext dataDbContext)
        {
            _dataDbContext = dataDbContext;
        }

        [BindProperty(SupportsGet = true)]
        public long Id { get; set; }

        [BindProperty]
        public string UserName { get; set; } = string.Empty;

        [BindProperty]
        public string Description { get; set; } = string.Empty;

        public AppUser? AppUser { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            AppUser? appUser = await _dataDbContext.AppUsers.AsNoTracking()
                                                            .Include(au => au.AppUserOptional)
                                                            .FirstOrDefaultAsync(au => au.AppUserId == Id);
            if (appUser != null)
            {
                AppUser = appUser;
                UserName = appUser.DisplayName;
                Description = appUser.Description;
                return Page();
            }
            return NotFound();
        }
    }
}
