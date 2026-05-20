using BlogApp.Models;
using BlogApp.Models.DataModels;
using BlogApp.Models.JoinModels;

namespace BlogApp.Controllers
{
    [Authorize(Roles = Constants.Roles.Members)]
    public class CommentOperationController : Controller
    {
        private DataDbContext _dataDbContext;
        private ILogger<BlogEditController> _logger;

        public CommentOperationController(DataDbContext dataDbContext, ILogger<BlogEditController> logger)
        {
            _dataDbContext = dataDbContext;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] int size = 5, [FromQuery] int page = 1)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            IQueryable<Comment> allUserComments = _dataDbContext.Comments.Include(c => c.Blog)
                                                                     .Where(c => c.AppUserId == identityAppUser!.AppUserId)
                                                                     .OrderByDescending(c => c.CreationDateTime)
                                                                     .AsNoTracking();

            Comment[] thisPageComments = await allUserComments.Skip((page - 1) * size).Take(size).ToArrayAsync();

            PaginateItemsViewModel<Comment> model = new(page, size, thisPageComments, await allUserComments.CountAsync());
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VoteAndReload([FromForm] CommentVoteDto voteDto)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            if (voteDto.AppUserId != identityAppUser.AppUserId) return RedirectToPage("/Account/AccessDenied");

            if (!ModelState.IsValid) return Redirect(voteDto.ReturnUrl);

            Comment? comment = await _dataDbContext.Comments.FirstOrDefaultAsync(c => c.CommentId == voteDto.CommentId);
            AppUser? appUser = HttpContext.GetAppUser();

            if (appUser != null && comment != null)
            {
                AppUserCommnet? commentVote = await _dataDbContext.AppUserComments.FirstOrDefaultAsync(cv => cv.CommentId == voteDto.CommentId && cv.AppUserId == voteDto.AppUserId);
                if (commentVote != null)// Update AppUserCommnet
                {
                    if (commentVote.VoteType == voteDto.VoteType) // Votes are equal, so user want to delete current vote
                    {
                        _dataDbContext.AppUserComments.Remove(commentVote);
                        await _dataDbContext.SaveChangesAsync();
                    }
                    else // Votes are not equal, so user want to update current vote
                    {
                        commentVote.VoteType = voteDto.VoteType;
                        _dataDbContext.AppUserComments.Update(commentVote);
                        await _dataDbContext.SaveChangesAsync();
                    }

                }
                else // Create new AppUserCommnet
                {
                    AppUserCommnet vote = new() { AppUserId = voteDto.AppUserId, CommentId = voteDto.CommentId, VoteType = voteDto.VoteType };
                    await _dataDbContext.AppUserComments.AddAsync(vote);
                    await _dataDbContext.SaveChangesAsync();
                }
                return Redirect(voteDto.ReturnUrl);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> AddAndReload([FromForm] CommentDto commentDto)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            if (commentDto.AppUserId != identityAppUser.AppUserId) return RedirectToPage("/Account/AccessDenied");

            if (!ModelState.IsValid) return Redirect(commentDto.ReturnUrl);

            AppUser? appUser = HttpContext.GetAppUser();
            Blog? blog = await _dataDbContext.Blogs.FirstOrDefaultAsync(b => b.BlogId == commentDto.BlogId);

            if (appUser != null && blog != null)
            {
                AppUserBlog? appUserBlog = await _dataDbContext.AppUserBlogs.FirstOrDefaultAsync(aub => aub.AppUserId == appUser.AppUserId && aub.BlogId == blog.BlogId);
                if (appUserBlog != null)
                {
                    if (appUserBlog.CommentId != null)
                    {
                        // user already have comment on this blog, so return:
                        return Redirect(commentDto.ReturnUrl);
                    }
                    else
                    {
                        Comment comment = new Comment()
                        {
                            AppUserId = appUser.AppUserId,
                            BlogId = blog.BlogId,
                            StringContent = commentDto.Text
                        };

                        var commentEntry = _dataDbContext.Comments.Add(comment);
                        await _dataDbContext.SaveChangesAsync();

                        appUserBlog.CommentId = commentEntry.Entity.CommentId;
                        _dataDbContext.AppUserBlogs.Update(appUserBlog);
                        await _dataDbContext.SaveChangesAsync();

                        return Redirect(commentDto.ReturnUrl);
                    }
                }
                else
                {
                    // create new AppUserBlog:
                    appUserBlog = new AppUserBlog()
                    {
                        AppUserId = appUser.AppUserId,
                        BlogId = blog.BlogId,
                        VoteType = Models.Enums.VoteType.Dislike,
                    };
                    await _dataDbContext.AppUserBlogs.AddAsync(appUserBlog);

                    Comment comment = new Comment()
                    {
                        AppUserId = appUser.AppUserId,
                        BlogId = blog.BlogId,
                        StringContent = commentDto.Text
                    };

                    var commentEntry = _dataDbContext.Comments.Add(comment);
                    await _dataDbContext.SaveChangesAsync();

                    appUserBlog.CommentId = commentEntry.Entity.CommentId;
                    _dataDbContext.AppUserBlogs.Update(appUserBlog);
                    await _dataDbContext.SaveChangesAsync();

                    return Redirect(commentDto.ReturnUrl);
                }
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(long id)
        {
            IdentityAppUser? identityAppUser = HttpContext.GetIdentityAppUser();
            if (identityAppUser == null) return RedirectToPage("/Account/AccessDenied");

            Comment? comment = await _dataDbContext.Comments.Include(c => c.AppUserCommnets)
                                                            .FirstOrDefaultAsync(c => c.CommentId == id);
            if (comment != null)
            {
                // Users only can delete comment just for themself, not others:
                if (comment.AppUserId != identityAppUser.AppUserId) return RedirectToPage("/Account/AccessDenied");

                // delete all its dependants             
                foreach (AppUserCommnet cv in comment.AppUserCommnets ?? Enumerable.Empty<AppUserCommnet>())
                {
                    _dataDbContext.AppUserComments.Remove(cv);
                }
                _dataDbContext.Comments.Remove(comment);

                await _dataDbContext.SaveChangesAsync();
                return RedirectToAction("Index", "CommentOperation");
            }
            else
                return NotFound();

        }
    }
}
