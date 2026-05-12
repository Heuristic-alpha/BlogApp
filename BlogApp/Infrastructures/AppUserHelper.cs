using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.DataModels;
using BlogApp.Models.JoinModels;

namespace BlogApp.Infrastructures
{
    public static class AppUserHelper
    {
        /// <summary>
        /// Create a new user in Identity and Data database.
        /// </summary>
        /// <param name="userManager"></param>
        /// <param name="dbContext"></param>
        /// <param name="userName"></param>
        /// <param name="email"></param>
        /// <param name="password"></param>
        /// <param name="isMale"></param>
        /// <param name="roles"></param>
        /// <returns>Result of async operation: True if successfull, False if unSuccessfull plus the list of errors.</returns>
        public static async Task<(bool, List<string>)> TryCreateUserAsync(UserManager<IdentityAppUser> userManager,
                                                                          DataDbContext dbContext,
                                                                          string userName,
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
            var createIdentityAppResult = await userManager.CreateAsync(identityUser, password);
            if (createIdentityAppResult.Succeeded)
            {
                IdentityAppUser? identityAppUser = await userManager.FindByEmailAsync(email);
                AppUser appUser = new AppUser()
                {
                    DisplayName = userName,
                    AppIdentityId = identityAppUser!.Id,
                    IsMale = isMale,
                    Description = password // Should remove on published
                };
                await dbContext.AppUsers.AddAsync(appUser);
                await dbContext.SaveChangesAsync();
                AppUser? user = await dbContext.AppUsers.FirstOrDefaultAsync(a => a.AppIdentityId == identityAppUser.Id);
                identityAppUser.AppUserId = user!.AppUserId;
                if (roles != null)
                {
                    IdentityResult addRoleResult = await userManager.AddToRolesAsync(identityAppUser, roles);
                    if (!addRoleResult.Succeeded)
                    {
                        foreach (var err in addRoleResult.Errors) errors.Add(err.Description);
                        return (false, errors);
                    }
                }
                var updateResult = await userManager.UpdateAsync(identityAppUser);
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

        //public static async Task<bool> TryDeleteUserAsync(UserManager<IdentityAppUser> userManager,
        //                                                  DataDbContext dbContext,
        //                                                  UserProfilePictureService profilePictureService,
        //                                                  string email)
        //{
        //    IdentityAppUser? identity = await userManager.FindByEmailAsync(email);
        //    if (identity != null)
        //    {
        //        var result = await userManager.DeleteAsync(identity);
        //        if (result != null)
        //        {
        //            AppUser? user = await dbContext.AppUsers.FirstOrDefaultAsync(a => a.AppIdentityId == identity.Id);
        //            dbContext.AppUsers.Remove(user!);
        //            await dbContext.SaveChangesAsync();
        //            return true;
        //        }
        //    }
        //    return false;
        //}
        public static async Task<bool> TryDeleteUserAsync(UserManager<IdentityAppUser> userManager,
                                                          DataDbContext dbContext,
                                                          UserProfilePictureService profilePictureService,
                                                          long appUserId)
        {
            // Query and delete object and all it dependents
            AppUser? appUser = await dbContext.AppUsers.Include(a => a.AppUserCommnets)
                                                       .Include(a => a.Blogs)
                                                       .Include(a => a.Comments)
                                                       .AsSplitQuery()
                                                       .FirstOrDefaultAsync(au => au.AppUserId == appUserId);
            if (appUser != null)
            {
                IdentityAppUser? identity = await userManager.FindByIdAsync(appUser.AppIdentityId!);
                if (identity != null)
                {
                    var result = await userManager.DeleteAsync(identity);
                    if (result != null)
                    {
                        await profilePictureService.RemoveProfilePictureAsync(appUserId);
                        foreach (Blog todo in appUser.Blogs ?? Enumerable.Empty<Blog>()) dbContext.Blogs.Remove(todo);
                        foreach (Comment comment in appUser.Comments ?? Enumerable.Empty<Comment>()) dbContext.Comments.Remove(comment);
                        foreach (AppUserCommnet cv in appUser.AppUserCommnets ?? Enumerable.Empty<AppUserCommnet>()) dbContext.AppUserComments.Remove(cv);

                        dbContext.AppUsers.Remove(appUser);
                        await dbContext.SaveChangesAsync();
                        return true;
                    }
                }
            }
            return false;
        }

        public static async Task<(bool, List<string>)> TryCreateUserAsync(UserManager<IdentityAppUser> userManager,
                                                                          DataDbContext dataDbContext,
                                                                          UserDetailsDto userDetailsDto)
        {

            var result = await TryCreateUserAsync(userManager,
                                                  dataDbContext,
                                                  userDetailsDto.UserName,
                                                  userDetailsDto.Email,
                                                  userDetailsDto.Password,
                                                  userDetailsDto.IsMale,
                                                  userDetailsDto.RolesDict?.Where(kvp => kvp.Value == true).Select(kvp => kvp.Key).ToArray());
            return result;
        }

        public static async Task<(bool, List<string>)> TryUpdateUserAsync(UserManager<IdentityAppUser> userManager,
                                                                          DataDbContext dataDbContext,
                                                                          UserProfilePictureService profilePictureService,
                                                                          long appUserId,
                                                                          string identityAppUserId,
                                                                          string newUserName,
                                                                          string newDescription,
                                                                          string newEmail,
                                                                          bool isMale,
                                                                          bool shouldDeleteProfilePicture,
                                                                          IDictionary<string, bool>? Roles)
        {
            List<string> errors = new List<string>();
            AppUser? appUser = await dataDbContext.AppUsers.FirstOrDefaultAsync(au => au.AppUserId == appUserId);
            IdentityAppUser? identityAppUser = await userManager.FindByIdAsync(identityAppUserId);
            if (appUser != null && identityAppUser != null)
            {
                var updateUserNameResult = await userManager.SetUserNameAsync(identityAppUser, newUserName);
                if (updateUserNameResult.Succeeded)
                {
                    var updateEmailResult = await userManager.SetEmailAsync(identityAppUser, newEmail);
                    if (updateEmailResult.Succeeded)
                    {
                        // update other properties
                        appUser.DisplayName = newUserName;
                        appUser.IsMale = isMale;
                        appUser.Description = newDescription;
                        await dataDbContext.SaveChangesAsync();

                        // update roles
                        if(Roles != null)
                        {
                            foreach (var role in Roles)
                            {
                                if (role.Value) // user should have this role
                                {
                                    if (await userManager.IsInRoleAsync(identityAppUser, role.Key))
                                    {
                                        // do nothing
                                    }
                                    else
                                    {
                                        await userManager.AddToRoleAsync(identityAppUser, role.Key);
                                    }
                                }
                                else // user should not have this role
                                {
                                    if (await userManager.IsInRoleAsync(identityAppUser, role.Key))
                                    {
                                        await userManager.RemoveFromRoleAsync(identityAppUser, role.Key);
                                    }
                                    else
                                    {
                                        // do nothing
                                    }
                                }
                            }
                        }

                        // update profile picture (for now we can only remove it)
                        if (shouldDeleteProfilePicture) await profilePictureService.RemoveProfilePictureAsync(appUserId);

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

        public static async Task<(bool, List<string>)> TryUpdateUserAsync(UserManager<IdentityAppUser> userManager,
                                                                          DataDbContext dataDbContext,
                                                                          UserProfilePictureService profilePictureService,
                                                                          UserDetailsDto userDetailsDto)
        {
            var result = await TryUpdateUserAsync(userManager,
                                                  dataDbContext,
                                                  profilePictureService,
                                                  userDetailsDto.AppUserId,
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
