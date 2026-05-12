using BlogApp.Models;

namespace BlogApp.Services
{
    public class UserProfilePictureService
    {
        private readonly string _directoryFullPath;
        private readonly string _directoryRelativePath = Path.Combine("images", "dynamic", "userPictures");
        private readonly string _wwwRootPath;
        private DataDbContext _dataDbContext;

        public UserProfilePictureService(DataDbContext dataDbContext, IHostEnvironment hostEnvironment)
        {
            _dataDbContext = dataDbContext;
            _wwwRootPath = Path.Combine(hostEnvironment.ContentRootPath, "wwwroot");
            _directoryFullPath = Path.Combine(_wwwRootPath, _directoryRelativePath);
            if (!Directory.Exists(_directoryFullPath))
            {
                Directory.CreateDirectory(_directoryFullPath);
            }
        }

        public async Task SavePictureAsync(IFormFile formFile, long appUserId)
        {
            AppUser appUser = await _dataDbContext.AppUsers.AsNoTracking()
                                                           .Include(au => au.AppUserOptional)
                                                           .FirstAsync(au => au.AppUserId == appUserId);

            string? profilePictureName = GetProfilePictureName(appUser, out string oldExtension);
            
            if (profilePictureName == null)
            {
                profilePictureName = Guid.NewGuid().ToString();
            }
            else // delete old file
            {
                string oldFileFullPath = Path.Combine(_directoryFullPath, profilePictureName + oldExtension);
                if (File.Exists(oldFileFullPath)) File.Delete(oldFileFullPath);
            }

            string newFileExtension = Path.GetExtension(formFile.FileName);
            string newRelativePath = Path.Combine(_directoryRelativePath, profilePictureName + newFileExtension);
            string newFullPath = Path.Combine(_directoryFullPath, profilePictureName + newFileExtension);

            using (FileStream fs = File.OpenWrite(newFullPath))
            {
                await formFile.CopyToAsync(fs);
            }

            AppUserOptional? appUserOptional = await _dataDbContext.AppUserOptionals.FirstOrDefaultAsync(auo => auo.AppUserId == appUserId);
            if (appUserOptional != null)
            {
                appUserOptional.ProfilePictureURL = newRelativePath;
                _dataDbContext.AppUserOptionals.Update(appUserOptional);
                await _dataDbContext.SaveChangesAsync();
            }
            else
            {
                appUserOptional = new AppUserOptional();
                appUserOptional.AppUserId = appUserId;
                appUserOptional.ProfilePictureURL = newRelativePath;
                await _dataDbContext.AppUserOptionals.AddAsync(appUserOptional);
                await _dataDbContext.SaveChangesAsync();
            }
        }

        public async Task<string> GetProfilePictureURLAsync(long appUserId)
        {
            AppUser? appUser = await _dataDbContext.AppUsers.AsNoTracking()
                                                            .Include(au => au.AppUserOptional)
                                                            .FirstOrDefaultAsync(au => au.AppUserId == appUserId);
            if (appUser != null)
            {
                if (appUser.AppUserOptional != null
                 && !string.IsNullOrEmpty(appUser.AppUserOptional.ProfilePictureURL))
                {
                    return appUser.AppUserOptional.ProfilePictureURL;
                }
                // return default pictures
                if (appUser.IsMale) return Constants.StaticImagesURL.MaleUserProfileIcon;
                else return Constants.StaticImagesURL.FemaleUserProfileIcon;
            }
            else return string.Empty;
        }

        public string GetProfilePictureURL(AppUser appUser)
        {
            if (appUser.AppUserOptional != null
             && !string.IsNullOrEmpty(appUser.AppUserOptional.ProfilePictureURL))
            {
                return appUser.AppUserOptional.ProfilePictureURL;
            }
            // return default pictures
            if (appUser.IsMale) return Constants.StaticImagesURL.MaleUserProfileIcon;
            else return Constants.StaticImagesURL.FemaleUserProfileIcon;

        }

        public async Task RemoveProfilePictureAsync(long appUserId)
        {
            AppUser? appUser = await _dataDbContext.AppUsers.Include(au => au.AppUserOptional)
                                                            .FirstOrDefaultAsync(au => au.AppUserId == appUserId);
            if (appUser != null && appUser.AppUserOptional != null)
            {
                string path = appUser.AppUserOptional.ProfilePictureURL;
                string fullPath = Path.Combine(_wwwRootPath, path);
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                appUser.AppUserOptional.ProfilePictureURL = string.Empty;
                _dataDbContext.AppUserOptionals.Update(appUser.AppUserOptional);
                await _dataDbContext.SaveChangesAsync();
            }
        }

        public async Task RemoveAllProfilePicturesAsync()
        {
            Directory.Delete(_directoryFullPath, true);

            AppUserOptional[] appUserOptionals = await _dataDbContext.AppUserOptionals.ToArrayAsync();
            foreach (var auo in appUserOptionals)
            {
                auo.ProfilePictureURL = string.Empty;
            }
            _dataDbContext.UpdateRange(appUserOptionals);
            await _dataDbContext.SaveChangesAsync();
        }

        private string? GetProfilePictureName(AppUser appUser, out string extension)
        {
            if (appUser.AppUserOptional != null)
            {
                string url = appUser.AppUserOptional.ProfilePictureURL;
                if (!string.IsNullOrEmpty(url))
                {
                    string fileName = Path.GetFileNameWithoutExtension(url);
                    extension = Path.GetExtension(url);
                    return fileName;
                }
            }
            extension = string.Empty;
            return null;
        }
    }
}
