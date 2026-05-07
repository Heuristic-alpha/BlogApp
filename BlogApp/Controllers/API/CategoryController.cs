using BlogApp.Models;
using BlogApp.Models.JoinModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    [Authorize(Roles = Constants.Roles.Admins, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private DataDbContext _dataDbContext;

        public CategoryController(DataDbContext dataDbContext)
        {
            _dataDbContext = dataDbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            Category[] categories = await _dataDbContext.Categories.AsNoTracking()
                                                                   .ToArrayAsync();
            return Ok(categories);
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            Category? category = await _dataDbContext.Categories.AsNoTracking()
                                                                .FirstOrDefaultAsync(c => c.CategoryId == id);

            if (category != null)
            {
                return Ok(category);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{blogId:long}")]
        public async Task<IActionResult> GetAllForBlog(long blogId)
        {
            Blog? blog = await _dataDbContext.Blogs.Include(b => b.BlogCategories!)
                                                   .ThenInclude(bc => bc.Category)
                                                   .AsNoTracking()
                                                   .FirstOrDefaultAsync(b => b.BlogId == blogId);
            if (blog != null)
            {
                // resolve circular refrence error
                foreach (BlogCategory bc in blog.BlogCategories ?? Enumerable.Empty<BlogCategory>())
                {
                    bc.Blog = null;
                    bc.Category!.BlogCategories = null;
                }

                Category[] categories = blog.BlogCategories!.Select(bc => bc.Category!).ToArray();
                return Ok(categories);
            }
            else return NotFound();
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Category category)
        {
            Category[] categories = await _dataDbContext.Categories.AsNoTracking()
                                                                   .ToArrayAsync();
            // new existingCategory should have none existing name, search for duplicate names:
            if (!categories.Any(c => string.Equals(category.Name, c.Name, StringComparison.InvariantCultureIgnoreCase)))
            {
                category.CategoryId = default;
                _dataDbContext.Categories.Add(category);
                await _dataDbContext.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return BadRequest($"Creating a new category failed, because there is existing category with name ({category.Name})");
            }
        }

        [HttpPut]
        public async Task<IActionResult> Update([FromBody] Category category)
        {
            // Query all categories
            Category[] categories = await _dataDbContext.Categories.ToArrayAsync();

            // Does this category exist?
            Category? existingCategory = categories.FirstOrDefault(c => c.CategoryId == category.CategoryId);

            if (existingCategory != null)
            {
                // search for duplicating names
                foreach (Category cat in categories)
                {
                    // exept itself
                    if (cat == existingCategory) continue;

                    if (cat.Name.Equals(category.Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return BadRequest($"Updating category failed, because there is existing category with name ({cat.Name})");
                    }
                }

                existingCategory.Name = category.Name;
                _dataDbContext.Categories.Update(existingCategory);
                await _dataDbContext.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            Category? category = await _dataDbContext.Categories.FirstOrDefaultAsync(c => c.CategoryId == id);
            if (category != null) 
            { 
                _dataDbContext.Categories.Remove(category);
                await _dataDbContext.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}
