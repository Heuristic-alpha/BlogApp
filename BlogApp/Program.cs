using BlogApp.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;

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
    IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(builder.Configuration["jwtSecret"]!)),
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
builder.Services.AddServerSideBlazor();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "Blog App", Version = "v1" });
    });
}
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<UrlLocator>();
builder.Services.AddScoped<UserProfilePictureService>();

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

app.UseStaticFiles();
app.UseAntiforgery();
app.UseAuthentication();
app.UseAuthorization();
app.UseIdentityAppUser();
app.MapControllers();
app.MapDefaultControllerRoute();
app.MapRazorPages();
app.MapBlazorHub();
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options => { options.SwaggerEndpoint("/swagger/v1/swagger.json", "Blog App"); });
}
app.UseBlazorFrameworkFiles("/webassembly");
app.MapFallbackToFile("/webassembly/{*path:nonfile}", "/webassembly/index.html");

await SeedDataDb.SeedingDataDb(app);

#endregion
app.Run();