using BlogApp.Models;
using BlogApp.Models.DataModels;
using BlogApp.Models.SearchModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Text;

namespace BlogApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class AppUserController : ControllerBase
    {
        private DataDbContext _dataDbContext;
        private UserManager<IdentityAppUser> _userManager;
        private RoleManager<IdentityRole> _roleManager;
        private AppUserManager _appUserManager;

        public AppUserController(DataDbContext dataDbContext, UserManager<IdentityAppUser> userManager, RoleManager<IdentityRole> roleManager, AppUserManager appUserManager)
        {
            _dataDbContext = dataDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _appUserManager = appUserManager;
        }

        /// <summary>
        /// Get All identity roles in server as array
        /// </summary>
        [Authorize(Roles = Constants.Roles.Admins, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
        [HttpGet]
        public async Task<IActionResult> GetAllIdentityRoles()
        {
            if (_roleManager.Roles.Any())
            {
                string?[] Roles = await _roleManager.Roles.Select(r => r.Name).ToArrayAsync();
                return Ok(Roles);
            }
            return NotFound();
        }

        /// <summary>
        /// Get Identity roles for specefic user as Dictionary<string,bool>
        /// </summary>
        [Authorize(Roles = Constants.Roles.Admins, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
        [HttpGet("{userId}")]
        public async Task<IActionResult> GetIdentityRoles(string userId)
        {
            // create empty dict
            if (userId == "0")
            {
                return Ok(GetEmptyRolesDict());
            }
            else
            {
                IdentityAppUser? identityAppUser = await _userManager.FindByIdAsync(userId);
                Dictionary<string, bool>? dict = await GetRolesForUserAsDictAsync(identityAppUser!);
                if (dict != null)
                    return Ok(dict);
                return NotFound();
            }
        }

        [Authorize(Roles = Constants.Roles.Admins, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
        [HttpPost] // HttpGet but because Get method dont support content in request, we should use Post
        public async Task<IActionResult> GetAll([FromBody] UserSearch userSearch)
        {
            IQueryable<IdentityAppUser> identityAppUsers = _userManager.Users.AsNoTracking();
            IQueryable<AppUser> appUsers = _dataDbContext.AppUsers.AsNoTracking();

            if (userSearch.UserId != null)
            {
                identityAppUsers = identityAppUsers.Where(iau => iau.AppUserId == userSearch.UserId);
                appUsers = appUsers.Where(au => au.AppUserId == userSearch.UserId);
            }
            if (userSearch.UserName != null)
            {
                identityAppUsers = identityAppUsers.Where(iau => EF.Functions.Like(iau.NormalizedUserName, $"%{userSearch.UserName.ToUpper()}%"));
                appUsers = appUsers.Where(au => EF.Functions.Like(au.DisplayName.ToUpper(), $"%{userSearch.UserName.ToUpper()}%"));
            }
            if (userSearch.Email != null)
            {
                identityAppUsers = identityAppUsers.Where(iau => EF.Functions.Like(iau.NormalizedEmail, $"%{userSearch.Email.ToUpper()}%"));
            }
            if (userSearch.IsMale != null) // if IsMale == null then should select all genders
            {
                appUsers = appUsers.Where(au => au.IsMale == userSearch.IsMale);
            }

            // Apply sorts
            identityAppUsers = identityAppUsers.OrderBy(iau => iau.AppUserId);
            appUsers = appUsers.OrderBy(au => au.AppUserId);

            IEnumerable<UserDetailsDto> userDetails = Enumerable.Join<IdentityAppUser, AppUser, long, UserDetailsDto>(identityAppUsers, appUsers, (iau) => iau.AppUserId, (au) => au.AppUserId, (IdentityAppUser iau, AppUser au) =>
            {
                return new UserDetailsDto()
                {
                    AppUserId = au.AppUserId,
                    IdentityAppUserId = iau.Id,
                    UserName = iau.UserName ?? au.DisplayName,
                    Email = iau.Email!,
                    IsMale = au.IsMale,
                    Description = au.Description,
                    CreationDateTime = au.CreationDateTime,
                    // Roles cant set here because async func can be used here
                    IdentityAppUser = iau, // only should set here
                };
            });

            if (userSearch.Skip != null)
            {
                userDetails = userDetails.Skip(userSearch.Skip.Value);
            }
            if (userSearch.Take != null)
            {
                userDetails = userDetails.Take(userSearch.Take.Value);
            }

            // Buffer results in memory
            UserDetailsDto[] bufferedResult = userDetails.ToArray();

            foreach (UserDetailsDto user in bufferedResult)
            {
                user.RolesDict = await GetRolesForUserAsDictAsync(user.IdentityAppUser!);
                // DANGER: Should IdentityAppUser set to null
                user.IdentityAppUser = null;
            }          

            if (bufferedResult != null)
            {
                return Ok(bufferedResult);
            }
            else
            {
                return NotFound();
            }
        }

        [Authorize(Roles = Constants.Roles.Admins, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
        [HttpGet("{id:long}")]
        public async Task<IActionResult> Get(long id)
        {
            AppUser? appUser = await _dataDbContext.AppUsers.FirstOrDefaultAsync(au => au.AppUserId == id);
            if (appUser != null)
            {
                IdentityAppUser? identityAppUser = await _userManager.FindByIdAsync(appUser.AppIdentityId!);
                if (identityAppUser != null)
                {
                    AppUserOptional? appUserOptional = await _dataDbContext.AppUserOptionals.AsNoTracking().FirstOrDefaultAsync(auo => auo.AppUserId == appUser.AppUserId);
                    UserDetailsDto userDetailsDto = new UserDetailsDto()
                    {
                        AppUserId = appUser.AppUserId,
                        IdentityAppUserId = identityAppUser.Id,
                        UserName = identityAppUser.UserName ?? appUser.DisplayName,
                        Email = identityAppUser.Email!,
                        IsMale = appUser.IsMale,
                        Description = appUser.Description,
                        CreationDateTime = appUser.CreationDateTime,
                        ProfilePictureURL = appUserOptional?.ProfilePictureURL ?? string.Empty,
                    };
                    userDetailsDto.RolesDict = await GetRolesForUserAsDictAsync(identityAppUser);

                    return Ok(userDetailsDto);
                }
            }
            return NotFound();
        }

        [Authorize(Roles = Constants.Roles.Manager, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
        [HttpDelete("{id:long}")]
        public async Task<IActionResult> Delete(long id)
        {
            if (await _appUserManager.TryDeleteUserAsync(id))
            {
                return Ok();
            }
            return NotFound();
        }

        [Authorize(Roles = Constants.Roles.Manager, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserDetailsDto userDetails)
        {
            if (userDetails != null)
            {
                (bool isSuccess, List<string> errors) = await _appUserManager.TryCreateUserAsync(userDetails);
                if (isSuccess)
                {
                    return Ok();
                }
                else
                {
                    StringBuilder errorMessage = new StringBuilder();
                    errorMessage.AppendLine("AppUserController: Creating user failed becuase of following errors:");
                    for (int i = 0; i < errors.Count; i++)
                    {
                        errorMessage.AppendLine($"({i + 1}) {errors[i]}");
                    }
                    return BadRequest(errorMessage.ToString());
                }
            }
            return BadRequest("AppUserController: Creating user failed because UserDetailsDto object is null");
        }

        [Authorize(Roles = Constants.Roles.Manager, AuthenticationSchemes = $"{JwtBearerDefaults.AuthenticationScheme}")]
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UserDetailsDto userDetails)
        {
            if (userDetails != null)
            {
                (bool isSuccess, List<string> errors) = await _appUserManager.TryUpdateUserAsync(userDetails);
                if (isSuccess)
                {
                    return Ok();
                }
                else
                {
                    StringBuilder errorMessage = new StringBuilder();
                    errorMessage.AppendLine("AppUserController: Updating user failed becuase of following errors:");
                    for (int i = 0; i < errors.Count; i++)
                    {
                        errorMessage.AppendLine($"({i + 1}) {errors[i]}");
                    }
                    return BadRequest(errorMessage.ToString());
                }
            }
            return BadRequest("AppUserController: Updating user failed because UserDetailsDto object is null");
        }

        private async Task<Dictionary<string, bool>?> GetRolesForUserAsDictAsync(IdentityAppUser identityAppUser)
        {
            if (identityAppUser != null)
            {
                IList<string> userRoles = await _userManager.GetRolesAsync(identityAppUser);
                Dictionary<string, bool> dict = _roleManager.Roles.ToDictionary<IdentityRole, string, bool>(
                    (i) => i.Name!,
                    (i) =>
                    {
                        foreach (string ur in userRoles)
                        {
                            if (ur.Equals(i.Name)) return true;
                        }
                        return false;
                    }, StringComparer.Ordinal);

                return dict;
            }
            return null;
        }

        private Dictionary<string, bool>? GetEmptyRolesDict()
        {
            return _roleManager.Roles.ToDictionary<IdentityRole, string, bool>(
                   (i) => i.Name!,
                   (i) => false,
                   StringComparer.Ordinal);
        }
    }
}
