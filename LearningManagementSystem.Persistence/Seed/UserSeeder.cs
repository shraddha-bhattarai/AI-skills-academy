using LearningManagementSystem.Persistence.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace LearningManagementSystem.Persistence.Seed;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(IServiceProvider serviceProvider)
    {
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        await SeedUsersAsync(userManager);
    }

    public static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        var defaultUsers = new[]
        {
            new
            {
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@lms.com",
                UserName = "admin",
                Password = "Admin@123",
                Role = "Admin"
            },
            new
            {
                FirstName = "Jane",
                LastName = "Doe",
                Email = "student@lms.com",
                UserName = "student",
                Password = "Student@123",
                Role = "Student"
            }
        };

        foreach (var u in defaultUsers)
        {
            var existingUser = await userManager.FindByEmailAsync(u.Email) 
                ?? await userManager.FindByNameAsync(u.UserName);

            if (existingUser == null)
            {
                var user = new ApplicationUser
                {
                    UserName = u.UserName,
                    Email = u.Email,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(user, u.Password);
                if (createResult.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, u.Role);
                }
            }
            else
            {
                if (!await userManager.IsInRoleAsync(existingUser, u.Role))
                {
                    await userManager.AddToRoleAsync(existingUser, u.Role);
                }
            }
        }
    }
}
