using BlogApp.Infrastructures;
using BlogApp.Models;
using BlogApp.Models.DataModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Controllers
{
    [Authorize(Roles = Constants.Roles.Admins, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppController : ControllerBase
    {
        private DataDbContext _dataDbContext;
        private IdentityContext _identityContext;

        public AppController(DataDbContext dataDbContext, IdentityContext identityContext)
        {
            _dataDbContext = dataDbContext;
            _identityContext = identityContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetDatabaseStatistic()
        {
            DataBaseStatisticDto dataBaseStatistic = new DataBaseStatisticDto();
            dataBaseStatistic.IdentityAppUserCount = await _identityContext.Users.CountAsync();
            dataBaseStatistic.AppUsersCount = await _dataDbContext.AppUsers.CountAsync();
            dataBaseStatistic.AppUserOptionalsCount = await _dataDbContext.AppUserOptionals.CountAsync();
            dataBaseStatistic.BlogsCount = await _dataDbContext.Blogs.CountAsync();
            dataBaseStatistic.CommentsCount = await _dataDbContext.Comments.CountAsync();
            dataBaseStatistic.AppUserCommentsCount = await _dataDbContext.AppUserComments.CountAsync();
            dataBaseStatistic.CategoriesCount = await _dataDbContext.Categories.CountAsync();
            dataBaseStatistic.BlogCategoriesCount = await _dataDbContext.BlogCategories.CountAsync();
            dataBaseStatistic.AppUserBlogsCount = await _dataDbContext.AppUserBlogs.CountAsync();

            return Ok(dataBaseStatistic);
        }
    }
}
