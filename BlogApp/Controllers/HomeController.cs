using BlogApp.Models;

namespace BlogApp.Controllers
{
    public class HomeController : Controller
    {
        private DataDbContext _dataDbContext;

        public HomeController(DataDbContext dataDbContext)
        {
            _dataDbContext = dataDbContext;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult About()
        {
            return View();
        }

        public async Task<IActionResult> BlogPublicView([FromRoute] long id, [FromQuery] string returnUrl)
        {
            Blog? blog = await _dataDbContext.Blogs.AsNoTracking()
                                                   .Include(b => b.AppUser!)
                                                   .ThenInclude(au => au.AppUserOptional)
                                                   .Include(b => b.Comments!)
                                                   .ThenInclude(c => c.AppUser)
                                                   .FirstOrDefaultAsync(b => b.BlogId == id && b.IsConfirmed == true && b.IsPublic == true);
            if (blog != null)
            {
                ViewBag.ReturnUrl = returnUrl;
                return View(blog);
            }
            else
                return NotFound();

        }

        public async Task<IActionResult> BlogListPublicView([FromQuery] int size = 10, [FromQuery] int page = 1)
        {
            var allUserBlogsQuery = _dataDbContext.Blogs.AsNoTracking()
                                                        .Where(b => b.IsConfirmed == true && b.IsPublic == true)
                                                        .Select(b => new PublicBlog()
                                                        {
                                                            BlogId = b.BlogId,
                                                            Title = b.Title,
                                                            Content = b.BlogContent,
                                                            Author = b.AppUser!.DisplayName,
                                                            PublishDate = b.CreationDateTime,
                                                            Categories = b.BlogCategories!.Select(bc => bc.Category!.Name).ToArray()
                                                        })
                                                        .OrderByDescending(pb => pb.PublishDate);

            var thisPageBlogs = await allUserBlogsQuery.Skip((page - 1) * size)
                                                       .Take(size)
                                                       .ToArrayAsync();

            PaginateItemsViewModel<PublicBlog> model = new(page, size, thisPageBlogs, await allUserBlogsQuery.CountAsync());
            return View(model);
        }
    }
}
