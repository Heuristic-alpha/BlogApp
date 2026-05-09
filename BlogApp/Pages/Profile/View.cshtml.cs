using BlogApp.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogApp.Pages.Profile
{
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

        public async Task<IActionResult> OnGetAsync()
        {
            AppUser? appUser = await _dataDbContext.AppUsers.AsNoTracking().FirstOrDefaultAsync(au => au.AppUserId == Id);
            if (appUser != null)
            {
                UserName = appUser.DisplayName;
                Description = appUser.Description;
                return Page();
            }
            return NotFound();
        }
    }
}
