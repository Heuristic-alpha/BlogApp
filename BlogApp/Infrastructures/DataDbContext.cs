using Microsoft.EntityFrameworkCore;
using BlogApp.Models;
using BlogApp.Models.JoinModels;

namespace BlogApp.Infrastructures
{
    public class DataDbContext : DbContext
    {
        public DataDbContext(DbContextOptions<DataDbContext> options) : base(options) { }

        public DbSet<AppUser> AppUsers => Set<AppUser>();
        public DbSet<Blog> Blogs => Set<Blog>();
        public DbSet<Comment> Comments => Set<Comment>();
        public DbSet<Category> Categories => Set<Category>();

        // Join Sets:
        public DbSet<AppUserCommnet> AppUserComments => Set<AppUserCommnet>();
        public DbSet<BlogCategory> BlogCategories => Set<BlogCategory>();

        // Additional Sets:
        public DbSet<AppUserOptional> AppUserOptionals => Set<AppUserOptional>();
    }
}
