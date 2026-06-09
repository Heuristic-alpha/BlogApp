using BlogApp.Models;
using BlogApp.Models.JoinModels;
using BlogApp.Models.SearchModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;

namespace BlogApp.Controllers
{
    [EnableRateLimiting(Constants.RateLimiterNames.AdminFixLimit)]
    [Authorize(Roles = Constants.Roles.Admins, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private DataDbContext _dataDbContext;
        private ILogger<CommentController> _logger;

        public CommentController(DataDbContext dbContext, ILogger<CommentController> logger)
        {
            _dataDbContext = dbContext;
            _logger = logger;
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            Comment? comment = await _dataDbContext.Comments.Include(c => c.AppUser)
                                                        .Include(c => c.AppUserCommnets)
                                                        .AsNoTracking()
                                                        .FirstOrDefaultAsync(c => c.CommentId == id);
            if (comment != null)
            {
                comment.AppUser!.Comments = null;
                foreach (AppUserCommnet commentVote in comment.AppUserCommnets ?? Enumerable.Empty<AppUserCommnet>())
                {
                    commentVote.Comment = null;
                }
                return Ok(comment);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost] // HttpGet but because Get method dont support content in request, we should use Post
        public async Task<IActionResult> GetAll([FromBody] CommentSearch commentSearch)
        {
            IQueryable<Comment> commentsQuery = _dataDbContext.Comments.Include(c => c.AppUser)
                                                                       .Include(c => c.AppUserCommnets)
                                                                       .AsNoTracking();

            if (commentSearch.CommentId != null) commentsQuery = commentsQuery.Where(cq => cq.CommentId == commentSearch.CommentId);
            if (commentSearch.BlogId != null) commentsQuery = commentsQuery.Where(cq => cq.BlogId == commentSearch.BlogId);
            if (commentSearch.AppUserId != null) commentsQuery = commentsQuery.Where(cq => cq.AppUserId == commentSearch.AppUserId);
            if (commentSearch.IsConfirmed != null) commentsQuery = commentsQuery.Where(cq => cq.IsConfirmed == commentSearch.IsConfirmed);
            if (commentSearch.CommentText != null) commentsQuery = commentsQuery.Where(cq => EF.Functions.Like(cq.StringContent, $"%{commentSearch.CommentText}%"));
            if (commentSearch.Skip != null) commentsQuery = commentsQuery.Skip(commentSearch.Skip.Value);
            if (commentSearch.Take != null) commentsQuery = commentsQuery.Take(commentSearch.Take.Value);
            commentsQuery = commentsQuery.OrderBy(cq => cq.CommentId).ThenBy(cq => cq.CreationDateTime);

            IEnumerable<Comment> bufferedComments = await commentsQuery.ToArrayAsync();
                     
            if (bufferedComments != null)
            {
                // resolve cyclic reference json error
                foreach (Comment comment in bufferedComments)
                {
                    comment.AppUser!.Comments = null;
                    foreach (AppUserCommnet commentVote in comment.AppUserCommnets ?? Enumerable.Empty<AppUserCommnet>())
                    {
                        commentVote.Comment = null;
                    }
                }
                return Ok(bufferedComments);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetAllForBlog(long id) // id is for Blog
        {
            Comment[] comments = await _dataDbContext.Comments.Include(c => c.AppUser)
                                                          .Include(c => c.AppUserCommnets)
                                                          .Where(c => c.BlogId == id)
                                                          .AsNoTracking()
                                                          .ToArrayAsync();
            if (comments != null)
            {
                // resolve cyclic reference json error
                foreach (Comment comment in comments)
                {
                    comment.AppUser!.Comments = null;
                    foreach (AppUserCommnet commentVote in comment.AppUserCommnets ?? Enumerable.Empty<AppUserCommnet>())
                    {
                        commentVote.Comment = null;
                    }
                }
                return Ok(comments);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpGet("{id:long}")]
        public async Task<IActionResult> GetAllForUser(long id) // id is for User
        {
            Comment[] comments = await _dataDbContext.Comments.Include(c => c.AppUser)
                                                          .Include(c => c.AppUserCommnets)
                                                          .Where(c => c.AppUserId == id)
                                                          .AsNoTracking()
                                                          .ToArrayAsync();
            if (comments != null)
            {
                // resolve cyclic reference json error
                foreach (Comment comment in comments)
                {
                    comment.AppUser!.Comments = null;
                    foreach (AppUserCommnet commentVote in comment.AppUserCommnets ?? Enumerable.Empty<AppUserCommnet>())
                    {
                        commentVote.Comment = null;
                    }
                }
                return Ok(comments);
            }
            else
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Comment comment)
        {
            // Validating:
            AppUser? appUser = await _dataDbContext.AppUsers.FirstOrDefaultAsync(a => a.AppUserId == comment.AppUserId);
            if (appUser == null) return BadRequest($"Creating comment failed: there is no AppUser with Id({comment.AppUserId.ToString()})");
            Blog? blog = await _dataDbContext.Blogs.FirstOrDefaultAsync(t => t.BlogId == comment.BlogId);
            if (blog == null) return BadRequest($"Creating comment failed: there is no Blog with Id({comment.BlogId.ToString()})");

            if (comment.CommentId == default)
            {
                _dataDbContext.Comments.Add(comment);
                await _dataDbContext.SaveChangesAsync();
                return Ok();
            }
            return BadRequest(ModelState);
        }

        [HttpPut]
        public async Task<IActionResult> Update(Comment comment)
        {
            // Validating:
            AppUser? appUser = await _dataDbContext.AppUsers.FirstOrDefaultAsync(a => a.AppUserId == comment.AppUserId);
            if (appUser == null) return BadRequest($"Updating comment failed: there is no AppUser with Id({comment.AppUserId.ToString()})");
            Blog? blog = await _dataDbContext.Blogs.FirstOrDefaultAsync(t => t.BlogId == comment.BlogId);
            if (blog == null) return BadRequest($"Updating comment failed: there is no Blog with Id({comment.BlogId.ToString()})");

            // Dissable navigation properties (it is possible the comment object has navigation properties that they confuse EF core)
            comment.AppUser = null;
            comment.Blog = null;

            if (comment.CommentId != default)
            {
                _dataDbContext.Comments.Update(comment);
                await _dataDbContext.SaveChangesAsync();
                return Ok();
            }
            return BadRequest(comment);
        }

        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            Comment? comment = await _dataDbContext.Comments.Include(c => c.AppUserCommnets)
                                                        .FirstOrDefaultAsync(c => c.CommentId == id);

            if (comment != null)
            {
                // First delete comment votes
                foreach (var cv in comment.AppUserCommnets!)
                {
                    _dataDbContext.AppUserComments.Remove(cv);
                }

                _dataDbContext.Comments.Remove(comment);
                await _dataDbContext.SaveChangesAsync();
                return Ok();
            }
            return NotFound();
        }
    }
}
