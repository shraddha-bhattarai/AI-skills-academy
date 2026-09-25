using Microsoft.AspNetCore.Mvc.Testing;

namespace LearningManagementSystem.Tests;

public class AuthorizationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthorizationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Guest_CannotAccess_StudentRoute()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Student/Home/Index");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task Guest_CannotAccess_AdminRoute()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Admin/Home/Index");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task Student_CannotAccess_AdminRoute()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestHelpers.LoginAsync(client, "student@lms.com", "Student@123");

        var response = await client.GetAsync("/Admin/Users/Index");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task Admin_CannotAccess_StudentOnlyRoute()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        await TestHelpers.LoginAsync(client, "admin@lms.com", "Admin@123");

        var response = await client.GetAsync("/Student/Home/Index");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/AccessDenied", response.Headers.Location?.ToString() ?? string.Empty);
    }

    [Fact]
    public async Task Guest_CannotReach_UnauthenticatedCrudUrls_ThatWereFixedInSession1()
    {
        // Root duplicate CRUD controllers (Quizzes/Enrollments/Certificates/etc.) were removed entirely
        // in Session 1; the equivalent Admin-area routes must require authentication.
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Admin/Certificates/Create");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("/Account/Login", response.Headers.Location?.ToString() ?? string.Empty);
    }
}
