using BlogApp.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogApp.Infrastructures.Middlewares
{
    public class AppUserIdentityInjectorMiddleware
    {
        public const string APP_USER_KEY = "__AppUser";
        public const string IDENTITY_APP_USER_KEY = "__IdentityAppUser";

        private RequestDelegate _next;

        public AppUserIdentityInjectorMiddleware(RequestDelegate requestDelegate)
        {
            _next = requestDelegate;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            IServiceProvider serviceProvider = httpContext.RequestServices.CreateScope().ServiceProvider;
            UserManager<IdentityAppUser> userManager = serviceProvider.GetRequiredService<UserManager<IdentityAppUser>>();
            DataDbContext dataDbContext = serviceProvider.GetRequiredService<DataDbContext>();

            IdentityAppUser? identityAppUser = await userManager.GetUserAsync(httpContext.User);
            if (identityAppUser != null)
            {
                AppUser appUser = await dataDbContext.AppUsers.FirstAsync(a => a.AppUserId == identityAppUser.AppUserId);

                httpContext.Items[APP_USER_KEY] = appUser;
                httpContext.Items[IDENTITY_APP_USER_KEY] = identityAppUser;
            }
            // goto next middleware
            await _next(httpContext);
        }
    }
}

namespace BlogApp
{
    public static class AppUserIdentityInjectorMiddlewareExtension
    {
        /// <summary>
        /// Register AppUserIdentityInjectorMiddleware to request pipline. (should be called after app.UseAuthentication() and app.UseAuthorization())
        /// </summary>       
        public static IApplicationBuilder UseIdentityAppUser(this IApplicationBuilder app)
        {
            return app.UseMiddleware<BlogApp.Infrastructures.Middlewares.AppUserIdentityInjectorMiddleware>();
        }

        /// <summary>
        /// Get current existing IdentityAppUser object
        /// </summary>       
        public static IdentityAppUser? GetIdentityAppUser(this HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(BlogApp.Infrastructures.Middlewares.AppUserIdentityInjectorMiddleware.IDENTITY_APP_USER_KEY, out object? value))
            {
                return value as IdentityAppUser;
            }
            return null;
        }

        /// <summary>
        /// Get current existing AppUser object
        /// </summary>       
        public static AppUser? GetAppUser(this HttpContext httpContext)
        {
            if (httpContext.Items.TryGetValue(BlogApp.Infrastructures.Middlewares.AppUserIdentityInjectorMiddleware.APP_USER_KEY, out object? value))
            {
                return value as AppUser;
            }
            return null;
        }
    }
}
