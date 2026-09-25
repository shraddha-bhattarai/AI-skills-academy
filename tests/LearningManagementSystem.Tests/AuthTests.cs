using Microsoft.AspNetCore.Mvc.Testing;

namespace LearningManagementSystem.Tests;

public class AuthTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_DuplicateEmail_IsRejected()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        var email = $"dup-{Guid.NewGuid():N}@example.com";

        async Task<HttpResponseMessage> RegisterAsync()
        {
            var page = await client.GetAsync("/Account/Register");
            var html = await page.Content.ReadAsStringAsync();
            var token = TestHelpers.ExtractAntiForgeryToken(html);

            var form = new Dictionary<string, string>
            {
                ["FirstName"] = "Test",
                ["LastName"] = "User",
                ["Email"] = email,
                ["Password"] = "Password1!",
                ["ConfirmPassword"] = "Password1!",
                ["__RequestVerificationToken"] = token
            };

            return await client.PostAsync("/Account/Register", new FormUrlEncodedContent(form));
        }

        var first = await RegisterAsync();
        Assert.Equal(System.Net.HttpStatusCode.Redirect, first.StatusCode); // success -> redirect to Login

        var second = await RegisterAsync();
        var secondHtml = await second.Content.ReadAsStringAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, second.StatusCode); // re-renders the form with a validation error, not a redirect
        Assert.Contains("already exists", secondHtml, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Login_CorrectCredentials_Succeeds()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await TestHelpers.LoginAsync(client, "student@lms.com", "Student@123");

        Assert.Equal(System.Net.HttpStatusCode.Redirect, response.StatusCode);
        Assert.True(response.Headers.Contains("Set-Cookie"), "A successful login should issue an auth cookie.");
    }

    [Fact]
    public async Task Login_IncorrectPassword_Fails()
    {
        var client = _factory.CreateClient();

        var response = await TestHelpers.LoginAsync(client, "student@lms.com", "WrongPassword123!");
        var html = await response.Content.ReadAsStringAsync();

        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode); // re-renders the login form, no redirect
        Assert.Contains("Invalid email or password", html);
    }
}
