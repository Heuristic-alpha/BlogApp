using BlogApp.Models;
using BlogApp.Models.JoinModels;
using Microsoft.AspNetCore.Identity;

namespace BlogApp.Infrastructures
{
    public static class SeedDbContext
    {
        public static async Task SeedingDataDb(WebApplication app)
        {
            var serviceProvider = app.Services.CreateScope().ServiceProvider;
            DataDbContext dataDb = serviceProvider.GetRequiredService<DataDbContext>();
            IdentityContext identityDb = serviceProvider.GetRequiredService<IdentityContext>();
            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            UserProfilePictureService userProfilePictureService = serviceProvider.GetRequiredService<UserProfilePictureService>();
            AppUserManager appUserManager = serviceProvider.GetRequiredService<AppUserManager>();

            ILogger<DataBaseSeedler> logger = app.Services.GetRequiredService<ILogger<DataBaseSeedler>>();

            dataDb.Database.EnsureDeleted();
            dataDb.Database.Migrate();
            identityDb.Database.EnsureDeleted();
            identityDb.Database.Migrate();

            await userProfilePictureService.RemoveAllProfilePicturesAsync();

            if (!roleManager.Roles.Any())
            {
                await roleManager.CreateAsync(new IdentityRole(Constants.Roles.Manager));
                await roleManager.CreateAsync(new IdentityRole(Constants.Roles.Admins));
                await roleManager.CreateAsync(new IdentityRole(Constants.Roles.Members));
            }

            if (!dataDb.AppUsers.Any())
            {                
                await appUserManager.TryCreateUserAsync("manager", "manager@gmail.com", "123456789", isMale: true, [Constants.Roles.Manager, Constants.Roles.Admins, Constants.Roles.Members]);
                await appUserManager.TryCreateUserAsync("admin", "admin@gmail.com", "123456789", isMale: true, [Constants.Roles.Admins, Constants.Roles.Members]);
                await appUserManager.TryCreateUserAsync("bob", "bob@gmail.com", "123456", isMale: true, [Constants.Roles.Members]);
                await appUserManager.TryCreateUserAsync("alice", "alice0@gmail.com", "123456", isMale: false, [Constants.Roles.Members]);
            }

            Category[] categories = Array.Empty<Category>();
            if (!dataDb.Categories.Any())
            {
                Category c1 = new Category() { Name = "Life" };
                Category c2 = new Category() { Name = "Technology" };
                Category c3 = new Category() { Name = "Game" };
                Category c4 = new Category() { Name = "Sport" };

                categories = [c1, c2, c3, c4];
                dataDb.Categories.AddRange(categories);
                dataDb.SaveChanges();
            }

            Blog[] blogs = Array.Empty<Blog>();
            if (!dataDb.Blogs.Any())
            {
                Blog blog1 = new Blog() { Title = "Programmer", BlogContent = "I want to be a great programmer.", AppUserId = 2, IsConfirmed = true, IsPublic = true };
                Blog blog2 = new Blog() { Title = "Programmer", BlogContent = "I want to be a great Web programmer.", AppUserId = 2, IsConfirmed = true, IsPublic = true };
                Blog blog3 = new Blog() { Title = "Life", BlogContent = "I want to make a lot of money.", AppUserId = 2, IsConfirmed = true, IsPublic = true };
                Blog blog4 = new Blog() { Title = "Programmer", BlogContent = "I want to be a great Backend programmer.", AppUserId = 3 };
                Blog blog5 = new Blog() { Title = "Life", BlogContent = "I want to buy a BIG house.", AppUserId = 3 };
                Blog blog6 = new Blog() { Title = "Life", BlogContent = "I want to buy a nice car (or nice super car).", AppUserId = 3 };
                Blog blog7 = new Blog() { Title = "Life", BlogContent = "I want to make my parrents happy.", AppUserId = 4 };
                Blog blog8 = new Blog() { Title = "Life", BlogContent = "I want to find nice GF and even possible marry her.", AppUserId = 4 };
                Blog blog9 = new Blog() { Title = "Site", BlogContent = "I have built this site ;)", AppUserId = 1, IsConfirmed = true, IsPublic = true };

                blogs = [blog1, blog2, blog3, blog4, blog5, blog6, blog7, blog8, blog9];

                dataDb.Blogs.AddRange(blogs);
                dataDb.SaveChanges();

                // create comments for some Blogs
                Comment comment1 = new Comment() { StringContent = "God I love this Blog 1 from admin", AppUserId = 2, BlogId = 1 };
                Comment comment2 = new Comment() { StringContent = "God I love this blog 1 from bob", AppUserId = 3, BlogId = 1 };
                Comment comment3 = new Comment() { StringContent = "God I love this blog 1 from manager", AppUserId = 1, BlogId = 1 };
                Comment comment4 = new Comment() { StringContent = "God I love this blog 4 from bob", AppUserId = 3, BlogId = 4 };
                Comment comment5 = new Comment() { StringContent = "God I love this blog 1 from bob", AppUserId = 3, BlogId = 1 };
                Comment comment6 = new Comment() { StringContent = "God I love this blog 2 from bob", AppUserId = 3, BlogId = 2 };

                Comment[] comments = [comment1, comment2, comment3, comment4, comment5, comment6];

                dataDb.Comments.AddRange(comments);
                dataDb.SaveChanges();
            }

            BlogCategory[] blogCategories = Array.Empty<BlogCategory>();
            if (!dataDb.BlogCategories.Any())
            {
                BlogCategory bc1 = new BlogCategory() { BlogId = 1, CategoryId = 3 };
                BlogCategory bc2 = new BlogCategory() { BlogId = 1, CategoryId = 2 };
                BlogCategory bc3 = new BlogCategory() { BlogId = 1, CategoryId = 4 };

                blogCategories = [bc1, bc2, bc3];
                dataDb.BlogCategories.AddRange(blogCategories);
                dataDb.SaveChanges();
            }
        }

        public class DataBaseSeedler { }
    }
}
