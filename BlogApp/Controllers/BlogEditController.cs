using BlogApp.Models;
using BlogApp.Models.DataModels;
using BlogApp.Models.JoinModels;

namespace BlogApp.Controllers
{
    [Authorize(Roles = Constants.Roles.Members)]
    public class BlogEditController : Controller
    {
        private DataDbContext _dataDbContext;
        private ILogger<BlogEditController> _logger;

        public BlogEditController(DataDbContext dbContext, ILogger<BlogEditController> logger)
        {
            _dataDbContext = dbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int size = 5, [FromQuery] int page = 1)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            IQueryable<Blog> allUserBlogs = _dataDbContext.Blogs.Include(b => b.AppUser)
                                                            .Where(b => b.AppUserId == identityAppUser.AppUserId)
                                                            .OrderByDescending(b => b.CreationDateTime)
                                                            .AsNoTracking();

            Blog[] thisPageBlogs = await allUserBlogs.Skip((page - 1) * size).Take(size).ToArrayAsync();

            PaginateItemsViewModel<Blog> model = new(page, size, thisPageBlogs, await allUserBlogs.CountAsync());
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Read(long id)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            Blog? blog = await _dataDbContext.Blogs.Include(b => b.AppUser!)
                                                        .ThenInclude(au => au.AppUserOptional)
                                                   .Include(b => b.Comments)
                                                   .Include(b => b.BlogCategories!)
                                                       .ThenInclude(bc => bc.Category)
                                                   .AsNoTracking()
                                                   .AsSplitQuery()
                                                   .FirstOrDefaultAsync(t => t.BlogId == id);
            if (blog != null)
            {
                return View(blog);
            }
            else return NotFound();
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            Category[] categories = await _dataDbContext.Categories.ToArrayAsync();
            EditBlogViewModel ebvm = new EditBlogViewModel()
            {
                Blog = new Blog() { Title = "", BlogContent = "", AppUserId = identityAppUser.AppUserId, IsConfirmed = false },
                CategoryNames = categories.Select(c => c.Name).ToArray(),
                CategoriesValue = categories.Select(c => false).ToArray(),
            };
            return View(ebvm);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromForm] EditBlogViewModel ebvm)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            // Users only can create new blog just for themself, not others:
            if (ebvm.Blog.AppUserId != identityAppUser.AppUserId) return RedirectToPage("/Account/AccessDenied");

            if (ModelState.IsValid)
            {
                // Set confirmation to false (just for safe)
                ebvm.Blog.IsConfirmed = false;

                // add blog
                var blogEntry = await _dataDbContext.Blogs.AddAsync(ebvm.Blog);
                await _dataDbContext.SaveChangesAsync();

                // add blogCategoriesToAdd
                long blogId = blogEntry.Entity.BlogId;
                List<BlogCategory> blogCategoriesToAdd = new();
                long[] categoryIds = await _dataDbContext.Categories.Select(c => c.CategoryId).ToArrayAsync();
                if (categoryIds.Length != ebvm.CategoriesValue.Length)
                {
                    // Role back the added blog
                    blogEntry.State = EntityState.Deleted;
                    await _dataDbContext.SaveChangesAsync();
                    _logger.LogError($"Creating BlogCategory(s) for new created blog (Id:{blogId}) failed because EditBlogViewModel.CategoriesValue.Length are not equal to CategoryIds.Length. ");
                    return BadRequest();
                }
                for (int i = 0; i < categoryIds.Length; i++)
                {
                    if (ebvm.CategoriesValue[i])
                    {
                        blogCategoriesToAdd.Add(new BlogCategory() { BlogId = blogId, CategoryId = categoryIds[i] });
                    }
                }
                await _dataDbContext.BlogCategories.AddRangeAsync(blogCategoriesToAdd);
                await _dataDbContext.SaveChangesAsync();

                return RedirectToAction("Index", "BlogEdit");
            }
            return View(ebvm);
        }

        [HttpGet]
        public async Task<IActionResult> Update(long id)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            Blog? b = await _dataDbContext.Blogs.FirstOrDefaultAsync(t => t.BlogId == id);
            if (b != null)
            {
                // Users only can update blog just for themself, not others:
                if (b.AppUserId != identityAppUser.AppUserId) return RedirectToPage("/Account/AccessDenied");

                BlogCategory[] blogCategories = await _dataDbContext.BlogCategories.Where(bc => bc.BlogId == b.BlogId).ToArrayAsync();
                Category[] allCategories = await _dataDbContext.Categories.ToArrayAsync();
                EditBlogViewModel ebvm = new EditBlogViewModel()
                {
                    Blog = b,
                    CategoryNames = allCategories.Select(c => c.Name).ToArray(),
                    CategoriesValue = allCategories.Select(c => blogCategories.Any(bc => bc.CategoryId == c.CategoryId)).ToArray(),
                };
                return View(ebvm);
            }
            else return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Update([FromForm] EditBlogViewModel ebvm)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            if (ModelState.IsValid)
            {
                // Users only can update blog just for themself, not others:
                if (ebvm.Blog.AppUserId != identityAppUser.AppUserId) return RedirectToPage("/Account/AccessDenied");

                // Set confirmation to false (just for safe)
                ebvm.Blog.IsConfirmed = false;

                // get last blog (if error accured, so can be roled back)
                Blog lastBlog = await _dataDbContext.Blogs.FirstAsync(b => b.BlogId == ebvm.Blog.BlogId);
                var lastBlogEntry = _dataDbContext.Entry(lastBlog);
                lastBlogEntry.State = EntityState.Detached;

                // update blog
                var blogEntry = _dataDbContext.Blogs.Update(ebvm.Blog);
                await _dataDbContext.SaveChangesAsync();

                // remove all prev BlogCategories for this blog
                BlogCategory[] blogCategoriesToDelete = await _dataDbContext.BlogCategories.Where(bc => bc.BlogId == ebvm.Blog.BlogId).ToArrayAsync();
                _dataDbContext.BlogCategories.RemoveRange(blogCategoriesToDelete);

                // add new blogCategoriesToAdd
                long blogId = blogEntry.Entity.BlogId;
                List<BlogCategory> blogCategoriesToAdd = new();
                long[] categoryIds = await _dataDbContext.Categories.Select(c => c.CategoryId).ToArrayAsync();
                if (categoryIds.Length != ebvm.CategoriesValue.Length)
                {
                    // Role back the updated blog
                    blogEntry.State = EntityState.Deleted;
                    await _dataDbContext.SaveChangesAsync();
                    await _dataDbContext.Blogs.AddAsync(lastBlog);
                    await _dataDbContext.SaveChangesAsync();
                    _logger.LogError($"Update BlogCategory(s) for blog (Id:{blogId}) failed because EditBlogViewModel.CategoriesValue.Length are not equal to CategoryIds.Length. ");
                    return BadRequest();
                }
                for (int i = 0; i < categoryIds.Length; i++)
                {
                    if (ebvm.CategoriesValue[i])
                    {
                        blogCategoriesToAdd.Add(new BlogCategory() { BlogId = blogId, CategoryId = categoryIds[i] });
                    }
                }
                await _dataDbContext.BlogCategories.AddRangeAsync(blogCategoriesToAdd);
                await _dataDbContext.SaveChangesAsync();

                return RedirectToAction("Index", "BlogEdit");
            }
            return View(ebvm);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            Blog? blog = await _dataDbContext.Blogs.Include(t => t.Comments!).ThenInclude(c => c.AppUserCommnets).FirstOrDefaultAsync(t => t.BlogId == id);
            if (blog != null)
            {
                // Users only can delete blog just for themself, not others:
                if (blog.AppUserId != identityAppUser.AppUserId) return RedirectToPage("/Account/AccessDenied");

                // delete all its dependants
                foreach (Comment comment in blog.Comments ?? Enumerable.Empty<Comment>())
                {
                    foreach (AppUserCommnet cv in comment.AppUserCommnets ?? Enumerable.Empty<AppUserCommnet>())
                    {
                        _dataDbContext.AppUserComments.Remove(cv);
                    }
                    _dataDbContext.Comments.Remove(comment);
                }

                _dataDbContext.Blogs.Remove(blog);
                await _dataDbContext.SaveChangesAsync();
                return RedirectToAction("Index", "BlogEdit");
            }
            else return NotFound();

        }

        [HttpPost]
        public async Task<IActionResult> VoteAndReload([FromForm] BlogVoteDto voteDto)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            if (voteDto.AppUserId != identityAppUser.AppUserId) return RedirectToPage("/Account/AccessDenied");

            if (!ModelState.IsValid) return Redirect(voteDto.ReturnUrl);

            Blog? blog = await _dataDbContext.Blogs.FirstOrDefaultAsync(b => b.BlogId == voteDto.BlogId);
            AppUser? appUser = HttpContext.GetAppUser();

            if (appUser != null && blog != null)
            {
                AppUserBlog? blogVote = await _dataDbContext.AppUserBlogs.FirstOrDefaultAsync(bv => bv.BlogId == voteDto.BlogId && bv.AppUserId == voteDto.AppUserId);
                if (blogVote != null)// Update AppUserBlog
                {
                    blogVote.VoteType = voteDto.VoteType;
                    _dataDbContext.AppUserBlogs.Update(blogVote);
                    await _dataDbContext.SaveChangesAsync();
                }
                else // Create new AppUserBlog
                {
                    AppUserBlog vote = new() { AppUserId = voteDto.AppUserId, BlogId = voteDto.BlogId, VoteType = voteDto.VoteType };
                    await _dataDbContext.AppUserBlogs.AddAsync(vote);
                    await _dataDbContext.SaveChangesAsync();
                }
                return Redirect(voteDto.ReturnUrl);
            }
            else
            {
                return NotFound();
            }
        }
    }
}
