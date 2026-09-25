using LearningManagementSystem.Persistence.Context;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LearningManagementSystem.Tests;

/// <summary>
/// Boots the real app (real Program.cs, real middleware pipeline, real Identity/role/auth setup)
/// but swaps the SQL Server DbContext for a fresh, isolated EF Core InMemory database per test class,
/// so tests never touch the real dev database. The app's own RoleSeeder/UserSeeder/DataSeeder still run
/// on startup (unmodified), which is what seeds the admin@lms.com / student@lms.com accounts tests use.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = "LmsTestDb_" + Guid.NewGuid();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseInMemoryDatabase(_dbName);
            });
        });
    }
}
