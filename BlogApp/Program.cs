using BlogApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);
#region builder configure region

builder.Services.AddDbContext<DataDbContext>(opts =>
{
    opts.UseSqlServer(connectionString: builder.Configuration["ConnectionStrings:DataDbConnection"]!);
    if (builder.Environment.IsDevelopment()) opts.EnableSensitiveDataLogging();
});
builder.Services.AddDbContext<IdentityContext>(opts =>
{
    opts.UseSqlServer(connectionString: builder.Configuration["ConnectionStrings:IdentityDbConnection"]!);
    if (builder.Environment.IsDevelopment()) opts.EnableSensitiveDataLogging();
});
builder.Services.AddIdentity<IdentityAppUser, IdentityRole>().AddEntityFrameworkStores<IdentityContext>();
builder.Services.Configure<IdentityOptions>(opts =>
{
    opts.Password.RequireDigit = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequiredLength = 6;

    opts.User.RequireUniqueEmail = true;
});

TokenValidationParameters JWTTokenValidationParameters = new TokenValidationParameters()
{
    ValidateIssuerSigningKey = true,
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration[Constants.JWTAuthentication.JWTSecretName]!)),
    ValidateAudience = false,
    ValidateIssuer = false
};
builder.Services.AddSingleton(JWTTokenValidationParameters);
builder.Services.AddAuthentication(opts =>
{
    opts.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
}).AddCookie(opts =>
{
    opts.Events.DisableRedirectForPath(e => e.OnRedirectToLogin, "/api", StatusCodes.Status401Unauthorized);
    opts.Events.DisableRedirectForPath(e => e.OnRedirectToAccessDenied, "/api", StatusCodes.Status403Forbidden);
}).AddJwtBearer(opts =>
{
    opts.RequireHttpsMetadata = false;
    opts.SaveToken = true;
    opts.TokenValidationParameters = JWTTokenValidationParameters;
    opts.Events = new JwtBearerEvents
    {
        // Look for Authorization Header for token
        OnTokenValidated = async ctx =>
        {
            var userManager = ctx.HttpContext.RequestServices.GetRequiredService<UserManager<IdentityAppUser>>();
            var signInManager = ctx.HttpContext.RequestServices.GetRequiredService<SignInManager<IdentityAppUser>>();

            // Find identity by username
            string? username = ctx.Principal?.FindFirst(ClaimTypes.Name)?.Value;
            IdentityAppUser? idUser = await userManager.FindByNameAsync(username ?? string.Empty);
            if (idUser == null)
            {
                // Find identity by email
                string? email = ctx.Principal?.FindFirst(ClaimTypes.Email)?.Value;
                idUser ??= await userManager.FindByEmailAsync(email ?? string.Empty);
            }

            // set User
            ctx.Principal = await signInManager.CreateUserPrincipalAsync(idUser!);
        },

        // Alternative location to look for token
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies[Constants.JWTAuthentication.JWTAuthToken];
            if (!string.IsNullOrEmpty(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };
});
builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blog App", Version = "v1" });
    });
}
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UrlLocator>();
builder.Services.AddScoped<AppUserManager>();
builder.Services.AddScoped<UserProfilePictureService>();
builder.Services.AddSingleton<LocalManager>();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(opts =>
{
    opts.Cookie.IsEssential = true;
});
builder.Services.AddRateLimiter(opts =>
{
    opts.AddFixedWindowLimiter(Constants.RateLimiterNames.PublicFixLimit, config =>
    {
        config.PermitLimit = 45;
        config.AutoReplenishment = true;
        config.QueueLimit = 0;
        config.Window = TimeSpan.FromMinutes(1);
    });

    opts.AddFixedWindowLimiter(Constants.RateLimiterNames.AdminFixLimit, config =>
    {
        config.PermitLimit = 100;
        config.AutoReplenishment = true;
        config.QueueLimit = 0;
        config.Window = TimeSpan.FromMinutes(1);
    });

    opts.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        string key = httpContext.Request.Headers["X-API-Key"].FirstOrDefault()
                     ?? httpContext.Connection.RemoteIpAddress?.ToString()
                     ?? "anonymous";
        return RateLimitPartition.GetFixedWindowLimiter(key, partition =>
        new FixedWindowRateLimiterOptions()
        {
            PermitLimit = 30,
            Window = TimeSpan.FromSeconds(60),
        });
    });

    opts.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync($"To many requests. Try again later.\n" +
                                                      $"{string.Join("\n", context.Lease.GetAllMetadata()
                                                                                       .Select(kvp => $"{kvp.Key} = {kvp.Value?.ToString()}"))}");
    };
});
#endregion
var app = builder.Build();
#region app configure region

if (app.Environment.IsProduction())
{
    app.UseExceptionHandler("/error");
}
else
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles(new StaticFileOptions()
{ 
    ServeUnknownFileTypes = true,
});
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.UseAntiforgery();
app.UseSession();
app.UseIdentityAppUser(); // Register 'AppUser' and 'IdentityAppUser' to HttpContext object
app.MapControllers();
app.MapDefaultControllerRoute();
app.MapRazorPages();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog App"); });

    app.MapGet("/hi", async (HttpContext context) =>
    {
        await context.Response.WriteAsJsonAsync($"Hello {(context.GetAppUser() != null ? context.GetAppUser()!.DisplayName : "anonymous")}");
    });
}
app.UseBlazorFrameworkFiles("/webassembly");
app.MapFallbackToFile("/webassembly/{*path:nonfile}", "/webassembly/index.html");

await SeedDbContext.SeedingDataDb(app);
await app.Services.GetRequiredService<LocalManager>().LoadFromServerFilePathAsync();

#endregion
app.Run();