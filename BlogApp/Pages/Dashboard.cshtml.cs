using BlogApp.Infrastructures;
using BlogApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace BlogApp.Pages
{
    [Authorize(Roles = Constants.Roles.Members)]
    public class DashboardModel : PageModel
    {
        private DataDbContext _dataDbContext;

        public DashboardModel(DataDbContext dataDbContext)
        {
            _dataDbContext = dataDbContext;
        }

        public AppUser? AppUser { get; set; }

        public IdentityAppUser? IdentityAppUser { get; set; }

        public Blog[] UserBlogs { get; set; } = Array.Empty<Blog>();

        public Comment[] UserComments { get; set; } = Array.Empty<Comment>();

        public bool CanShowBlogs { get; set; } = false;
        public bool CanShowComments { get; set; } = false;

        public async Task<IActionResult> OnGetAsync([FromQuery] bool canShowBlogs, [FromQuery] bool canShowComments)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser != null)
            {
                CanShowBlogs = canShowBlogs;
                CanShowComments = canShowComments;
                IdentityAppUser = identityAppUser;
                AppUser = HttpContext.GetAppUser();
                if (CanShowBlogs)
                {
                    UserBlogs = await _dataDbContext.Blogs.Include(b => b.AppUser)
                                                          .Include(b => b.Comments)    
                                                          .AsNoTracking()
                                                          .Where(b => b.AppUserId == identityAppUser.AppUserId)
                                                          .ToArrayAsync();                   
                }
                if (CanShowComments)
                {
                    UserComments = await _dataDbContext.Comments.Include(c => c.AppUser)
                                                                .Include(c => c.Blog)
                                                                .Include(c => c.AppUserCommnets)
                                                                .AsSplitQuery()
                                                                .AsNoTracking()
                                                                .Where(c => c.AppUserId == identityAppUser.AppUserId)
                                                                .ToArrayAsync();                   
                }                
                return Page();
            }
            else return RedirectToPage("/Account/AccessDenied");
        }
    }
}
