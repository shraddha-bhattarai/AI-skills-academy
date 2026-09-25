namespace LearningManagementSystem.Application.Common;

public static class UserNameHelper
{
    public static string? ResolveUserName(IReadOnlyList<LookupItemDto> users, string userId)
    {
        return users.FirstOrDefault(u => u.Id == userId)?.Name;
    }

    public static void ApplyUserNames<T>(
        IEnumerable<T> items,
        IReadOnlyList<LookupItemDto> users,
        Func<T, string> getUserId,
        Action<T, string> setUserName)
    {
        var lookup = users.ToDictionary(u => u.Id, u => u.Name);

        foreach (var item in items)
        {
            var userId = getUserId(item);
            if (lookup.TryGetValue(userId, out var name))
            {
                setUserName(item, name);
            }
        }
    }
}
