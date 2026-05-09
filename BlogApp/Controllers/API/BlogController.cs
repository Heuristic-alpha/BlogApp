using BlogApp.Models;
using BlogApp.Models.JoinModels;
using BlogApp.Models.SearchModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace BlogApp.Controllers
{
    [Authorize(Roles = Constants.Roles.Admins, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class BlogController : ControllerBase
    {
        private DataDbContext _dataDbContext;
        private ILogger<BlogController> _logger;

        public BlogController(DataDbContext dbContext, ILogger<BlogController> logger)
        {
            _dataDbContext = dbContext;
            _logger = logger;
        }

        [HttpPost] // HttpGet but because Get method dont support content in request, we should use Post
        public async Task<IActionResult> GetAll([FromBody] BlogSearch blogSearch)
        {
            IQueryable<Blog> blogsQuery = _dataDbContext.Blogs.Include(t => t.AppUser)
                                                              .AsNoTracking();

            if (blogSearch.BlogId != null) blogsQuery = blogsQuery.Where(b => b.BlogId == blogSearch.BlogId);
            if (blogSearch.AppUserId != null) blogsQuery = blogsQuery.Where(b => b.AppUserId == blogSearch.AppUserId);
            if (blogSearch.IsConfirmed != null) blogsQuery = blogsQuery.Where(b => b.IsConfirmed == blogSearch.IsConfirmed);
            if (blogSearch.IsPublic != null) blogsQuery = blogsQuery.Where(b => b.IsPublic == blogSearch.IsPublic);
            if (blogSearch.Title != null) blogsQuery = blogsQuery.Where(b => EF.Functions.Like(b.Title, $"%{blogSearch.Title}%"));
            if (blogSearch.Content != null) blogsQuery = blogsQuery.Where(b => EF.Functions.Like(b.BlogContent, $"%{blogSearch.Content}%"));
            if (blogSearch.Skip != null) blogsQuery = blogsQuery.Skip(blogSearch.Skip.Value);
            if (blogSearch.Take != null) blogsQuery = blogsQuery.Take(blogSearch.Take.Value);
            blogsQuery = blogsQuery.OrderBy(b => b.BlogId).ThenBy(b => b.CreationDateTime);
          
            IEnumerable<Blog> bufferedBlogs = await blogsQuery.ToArrayAsync();
           
            if (bufferedBlogs != null)
            {
                foreach (var blog in bufferedBlogs)
                {
                    if (blog.AppUser != null)
                    {
                        blog.AppUser.Blogs = null;
                    }
                }
                return Ok(bufferedBlogs);
            }
            return NotFound();
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetAllForUser(long id)
        {
            Blog[]? blogs = await _dataDbContext.Blogs.Include(b => b.AppUser)
                                                      .AsNoTracking()
                                                      .Where(b => b.AppUserId == id)
                                                      .ToArrayAsync();
            if (blogs != null)
            {
                foreach (var blog in blogs)
                {
                    if (blog.AppUser != null)
                    {
                        blog.AppUser.Blogs = null;
                    }
                }
                return Ok(blogs);
            }
            return NotFound();
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            Blog? blog = await _dataDbContext.Blogs.Include(b => b.AppUser)
                                                   .Include(b => b.BlogCategories!)
                                                   .ThenInclude(bc => bc.Category)
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(t => t.BlogId == id);
            if (blog != null)
            {
                if (blog.AppUser != null)
                {
                    blog.AppUser.Blogs = null;
                }
                if (blog.BlogCategories != null)
                {
                    foreach (var bc in blog.BlogCategories)
                    {
                        bc.Category!.BlogCategories = null;
                        bc.Blog = null;
                    }
                }
                return Ok(blog);
            }
            return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Blog blog)
        {
            if (blog != null && blog.BlogId == default)
            {
                // first create blog then query it's PK and assigned to BlogCategories, then add them to database:
                IEnumerable<BlogCategory> blogCategories = blog.BlogCategories ?? Enumerable.Empty<BlogCategory>();
                blog.BlogCategories = null;
                var blogEntry = await _dataDbContext.Blogs.AddAsync(blog);
                await _dataDbContext.SaveChangesAsync();
                long blogId = blogEntry.Entity.BlogId;
                foreach (BlogCategory bc in blogCategories) bc.BlogId = blogId;
                await _dataDbContext.BlogCategories.AddRangeAsync(blogCategories);
                await _dataDbContext.SaveChangesAsync();

                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Blog blog)
        {
            if (blog != null && blog.BlogId != default)
            {
                // Resolve EF Core cyclic problems
                blog.AppUser = null;
                blog.Comments = null;

                // Delete old records
                BlogCategory[] blogCategoriesToDelete = await _dataDbContext.BlogCategories.Where(bc => bc.BlogId == blog.BlogId).ToArrayAsync();
                _dataDbContext.BlogCategories.RemoveRange(blogCategoriesToDelete);

                // Add new records
                BlogCategory[] blogCategoriesToAdded = blog.BlogCategories!.ToArray();
                foreach (BlogCategory bc in blogCategoriesToAdded)
                {
                    bc.Category = null;
                    bc.Blog = null;
                }
                await _dataDbContext.BlogCategories.AddRangeAsync(blogCategoriesToAdded);

                blog.BlogCategories = null;
                _dataDbContext.Blogs.Update(blog);
                await _dataDbContext.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (id != default)
            {
                Blog? blog = await _dataDbContext.Blogs.Include(t => t.Comments!)
                                                   .ThenInclude(c => c.AppUserCommnets)
                                                   .FirstOrDefaultAsync(t => t.BlogId == id);
                if (blog != null)
                {
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
                    return Ok();
                }
            }
            return BadRequest();
        }
    }
}
