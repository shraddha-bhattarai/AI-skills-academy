using System.Text.RegularExpressions;

namespace LearningManagementSystem.Tests;

public static class TestHelpers
{
    private static readonly Regex TokenRegex = new(
        "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"",
        RegexOptions.Compiled);

    public static string ExtractAntiForgeryToken(string html)
    {
        var match = TokenRegex.Match(html);
        if (!match.Success)
        {
            throw new InvalidOperationException("Could not find an antiforgery token in the response HTML.");
        }

        return match.Groups[1].Value;
    }

    public static async Task<HttpResponseMessage> LoginAsync(HttpClient client, string email, string password)
    {
        var loginPage = await client.GetAsync("/Account/Login");
        var loginHtml = await loginPage.Content.ReadAsStringAsync();
        var token = ExtractAntiForgeryToken(loginHtml);

        var form = new Dictionary<string, string>
        {
            ["Email"] = email,
            ["Password"] = password,
            ["__RequestVerificationToken"] = token
        };

        return await client.PostAsync("/Account/Login", new FormUrlEncodedContent(form));
    }
}
