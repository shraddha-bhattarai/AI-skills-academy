namespace LearningManagementSystem.Application.Common;

public static class UrlSafety
{
    public static bool IsSafeHttpUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return false;
        }

        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }

    public static string? SanitizeOrNull(string? url) => IsSafeHttpUrl(url) ? url : null;
}
