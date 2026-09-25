using LearningManagementSystem.Application;
using LearningManagementSystem.Infrastructure;
using LearningManagementSystem.Persistence;
using LearningManagementSystem.Persistence.Context;
using LearningManagementSystem.Persistence.Identity;
using LearningManagementSystem.Persistence.Seed;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuestPDF.Infrastructure;

QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Generous headroom above the largest business-rule file-size check in the app (Assignment
// submissions: 25MB). Kestrel's own default (~28.6MB) sits too close to that limit — a file
// just over 25MB would cross Kestrel's ceiling too, and Kestrel enforces its limit by aborting
// the TCP connection mid-upload rather than returning a clean response, which looks like the
// app crashed even though nothing broke server-side. Raising it here ensures the app's own
// graceful, friendly validation is always what a student sees for an oversized upload.
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 50 * 1024 * 1024; // 50 MB
});

builder.Services.AddControllersWithViews();
builder.Services.AddPersistence(builder.Configuration);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var jwtSettings = builder.Configuration.GetSection("Jwt");
var jwtKey = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = IdentityConstants.ApplicationScheme;
    options.DefaultChallengeScheme = IdentityConstants.ApplicationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(jwtKey)
    };
});

builder.Services.AddAuthorization();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 6;
    options.SignIn.RequireConfirmedAccount = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var database = services.GetRequiredService<ApplicationDbContext>();
    // The local file is made on first run. Existing SQL Server installations still use
    // the project's SQL Server migrations and keep their existing records.
    if (database.Database.IsSqlite())
        await database.Database.EnsureCreatedAsync();
    else if (database.Database.IsSqlServer())
        await database.Database.MigrateAsync();

    await RoleSeeder.SeedRolesAsync(services);
    await UserSeeder.SeedUsersAsync(services);
    await DataSeeder.SeedDataAsync(services);
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Without this, a request rejected before it reaches a controller (an oversized upload,
// a stale/missing antiforgery token, etc.) returns a bare empty-body 400/413 that looks
// like the app crashed. Re-execute those status codes through Home/Error so the user always
// sees a real page instead of a blank response.
app.UseStatusCodePagesWithReExecute("/Home/Error", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "areas",
    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

// Exposes the top-level-statement Program class to WebApplicationFactory<Program> in the test project.
// No behavior change — purely a testability marker.
public partial class Program { }
