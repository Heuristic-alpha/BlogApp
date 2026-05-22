using BlogApp.Models;
using BlogApp.Models.DataModels;
using BlogApp.Models.JoinModels;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Services
{
    public class AppUserManager
    {
        private UserManager<IdentityAppUser> _userManager;
        private DataDbContext _dataDbContext;
        private UserProfilePictureService _userProfilePictureService;

        public AppUserManager(UserManager<IdentityAppUser> userManager, DataDbContext dataDbContext, UserProfilePictureService userProfilePictureService)
        {
            _userManager = userManager;
            _dataDbContext = dataDbContext;
            _userProfilePictureService = userProfilePictureService;
        }


        public async Task<(bool, List<string>)> TryCreateUserAsync(string userName,
                                                                   string email,
                                                                   string password,
                                                                   bool isMale,
                                                                   string[]? roles)
        {
            List<string> errors = new List<string>();

            IdentityAppUser identityUser = new IdentityAppUser()
            {
                UserName = userName,
                Email = email,
            };
            var createIdentityAppResult = await _userManager.CreateAsync(identityUser, password);
            if (createIdentityAppResult.Succeeded)
            {
                IdentityAppUser? identityAppUser = await _userManager.FindByEmailAsync(email);
                AppUser appUser = new AppUser()
                {
                    DisplayName = userName,
                    AppIdentityId = identityAppUser!.Id,
                    IsMale = isMale,
                    Description = string.Empty
                };
                await _dataDbContext.AppUsers.AddAsync(appUser);
                await _dataDbContext.SaveChangesAsync();
                AppUser? user = await _dataDbContext.AppUsers.FirstOrDefaultAsync(a => a.AppIdentityId == identityAppUser.Id);
                identityAppUser.AppUserId = user!.AppUserId;
                if (roles != null)
                {
                    IdentityResult addRoleResult = await _userManager.AddToRolesAsync(identityAppUser, roles);
                    if (!addRoleResult.Succeeded)
                    {
                        foreach (var err in addRoleResult.Errors) errors.Add(err.Description);
                        return (false, errors);
                    }
                }
                var updateResult = await _userManager.UpdateAsync(identityAppUser);
                if (!updateResult.Succeeded)
                {
                    foreach (var err in updateResult.Errors) errors.Add(err.Description);
                    return (false, errors);
                }

                // Successfull return
                return (true, errors);
            }
            else
            {
                foreach (var err in createIdentityAppResult.Errors) errors.Add(err.Description);
                return (false, errors);
            }
        }

        public async Task<(bool, List<string>)> TryCreateUserAsync(UserDetailsDto userDetailsDto)
        {
            var result = await TryCreateUserAsync(userDetailsDto.UserName,
                                                  userDetailsDto.Email,
                                                  userDetailsDto.Password,
                                                  userDetailsDto.IsMale,
                                                  userDetailsDto.RolesDict?.Where(kvp => kvp.Value == true).Select(kvp => kvp.Key).ToArray());
            return result;
        }

        public async Task<bool> TryDeleteUserAsync(long appUserId)
        {
            // Query and delete object and all it dependents
            AppUser? appUser = await _dataDbContext.AppUsers.Include(a => a.Blogs)
                                                            .Include(a => a.Comments)
                                                            .FirstOrDefaultAsync(au => au.AppUserId == appUserId);
            if (appUser != null)
            {
                IdentityAppUser? identity = await _userManager.FindByIdAsync(appUser.AppIdentityId!);
                if (identity != null)
                {
                    var result = await _userManager.DeleteAsync(identity);
                    if (result != null)
                    {
                        await _userProfilePictureService.RemoveProfilePictureAsync(appUserId);
                        foreach (Blog todo in appUser.Blogs ?? Enumerable.Empty<Blog>()) _dataDbContext.Blogs.Remove(todo);
                        foreach (Comment comment in appUser.Comments ?? Enumerable.Empty<Comment>()) _dataDbContext.Comments.Remove(comment);
                        foreach (AppUserCommnet cv in appUser.AppUserCommnets ?? Enumerable.Empty<AppUserCommnet>()) _dataDbContext.AppUserComments.Remove(cv);

                        _dataDbContext.AppUsers.Remove(appUser);
                        await _dataDbContext.SaveChangesAsync();
                        return true;
                    }
                }
            }
            return false;
        }


        public async Task<(bool, List<string>)> TryUpdateUserAsync(long appUserId,
                                                                   string identityAppUserId,
                                                                   string newUserName,
                                                                   string newDescription,
                                                                   string newEmail,
                                                                   bool isMale,
                                                                   bool shouldDeleteProfilePicture,
                                                                   IDictionary<string, bool>? Roles)
        {
            List<string> errors = new List<string>();
            AppUser? appUser = await _dataDbContext.AppUsers.FirstOrDefaultAsync(au => au.AppUserId == appUserId);
            IdentityAppUser? identityAppUser = await _userManager.FindByIdAsync(identityAppUserId);
            if (appUser != null && identityAppUser != null)
            {
                var updateUserNameResult = await _userManager.SetUserNameAsync(identityAppUser, newUserName);
                if (updateUserNameResult.Succeeded)
                {
                    var updateEmailResult = await _userManager.SetEmailAsync(identityAppUser, newEmail);
                    if (updateEmailResult.Succeeded)
                    {
                        // update other properties
                        appUser.DisplayName = newUserName;
                        appUser.IsMale = isMale;
                        appUser.Description = newDescription;
                        await _dataDbContext.SaveChangesAsync();

                        // update roles
                        if (Roles != null)
                        {
                            foreach (var role in Roles)
                            {
                                if (role.Value) // user should have this role
                                {
                                    if (await _userManager.IsInRoleAsync(identityAppUser, role.Key))
                                    {
                                        // do nothing
                                    }
                                    else
                                    {
                                        await _userManager.AddToRoleAsync(identityAppUser, role.Key);
                                    }
                                }
                                else // user should not have this role
                                {
                                    if (await _userManager.IsInRoleAsync(identityAppUser, role.Key))
                                    {
                                        await _userManager.RemoveFromRoleAsync(identityAppUser, role.Key);
                                    }
                                    else
                                    {
                                        // do nothing
                                    }
                                }
                            }
                        }

                        // update profile picture (for now we can only remove it)
                        if (shouldDeleteProfilePicture) await _userProfilePictureService.RemoveProfilePictureAsync(appUserId);

                        // Successfull operation:
                        return (true, errors);
                    }
                    else
                    {
                        errors.AddRange(updateEmailResult.Errors.Select(e => e.Description));
                        return (false, errors);
                    }
                }
                else
                {
                    errors.AddRange(updateUserNameResult.Errors.Select(e => e.Description));
                    return (false, errors);
                }
            }
            else
            {
                errors.Add("Unable to update user, because user not exist in database.");
                return (false, errors);
            }
        }

        public async Task<(bool, List<string>)> TryUpdateUserAsync(UserDetailsDto userDetailsDto)
        {
            var result = await TryUpdateUserAsync(userDetailsDto.AppUserId,
                                                  userDetailsDto.IdentityAppUserId,
                                                  userDetailsDto.UserName,
                                                  userDetailsDto.Description ?? string.Empty,
                                                  userDetailsDto.Email,
                                                  userDetailsDto.IsMale,
                                                  string.IsNullOrEmpty(userDetailsDto.ProfilePictureURL),
                                                  userDetailsDto.RolesDict);
            return result;
        }
    }
}
