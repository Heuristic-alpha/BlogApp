using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using BlogApp.Infrastructures;
using BlogApp.Models;

namespace BlogApp.ViewComponents
{
    public class RenderBlog : ViewComponent
    {
        private DataDbContext _dataDbContext;

        public RenderBlog(DataDbContext dbContext)
        {
            _dataDbContext = dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(Blog blog)
        {           
            if(blog != null && (blog.AppUser == null || blog.Comments == null || blog.AppUserBlogs == null))
            {
                blog = await _dataDbContext.Blogs.AsNoTracking()
                                                 .Include(b => b.AppUser!)
                                                 .ThenInclude(au => au.AppUserOptional)
                                                 .Include(b => b.Comments!)
                                                 .ThenInclude(c => c.AppUserCommnets)
                                                 .Include(b => b.AppUserBlogs)
                                                 .FirstAsync(b => b.BlogId == blog.BlogId);
            }
            TempData["Categories"] = await _dataDbContext.BlogCategories.AsNoTracking()
                                                                        .Include(bc => bc.Category)
                                                                        .Where(bc => bc.BlogId == blog!.BlogId)
                                                                        .Select(bc => bc.Category!.Name)
                                                                        .ToArrayAsync();

            AppUser? appUser = HttpContext.GetAppUser();
            if(appUser != null)
            {
                TempData["CurrentAppUserBlog"] = await _dataDbContext.AppUserBlogs.AsNoTracking()
                                                                                  .FirstOrDefaultAsync(aub => aub.AppUserId == appUser.AppUserId && aub.BlogId == blog!.BlogId);
            }
           

            return View(blog);
        }
    }
}

