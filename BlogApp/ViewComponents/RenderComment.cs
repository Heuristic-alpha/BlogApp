using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using BlogApp.Infrastructures;
using BlogApp.Models;

namespace BlogApp.ViewComponents
{
    public class RenderComment : ViewComponent
    {
        private DataDbContext _dbContext;

        public RenderComment(DataDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IViewComponentResult> InvokeAsync(Comment comment, bool canAddLike = false)
        {          
            if((comment != null) && (comment.AppUser == null || comment.AppUserCommnets == null))
            {
                comment = await _dbContext.Comments.AsNoTracking()
                                                   .Include(c => c.AppUser)
                                                   .Include(c => c.AppUserCommnets)
                                                   .FirstAsync(c => c.CommentId == comment.CommentId);
            }           
           
            ViewBag.CanAddLike = canAddLike;
            return View(comment);
        }
    }
}
