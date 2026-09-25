using LearningManagementSystem.Application.Common;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace LearningManagementSystem.Web.Extensions;

public static class SelectListExtensions
{
    public static SelectList ToSelectList(this IReadOnlyList<LookupItemDto> items, string? selectedValue = null)
    {
        return new SelectList(items, nameof(LookupItemDto.Id), nameof(LookupItemDto.Name), selectedValue);
    }

    public static SelectList ToSelectList(this IReadOnlyList<LookupItemDto> items, int? selectedValue)
    {
        return items.ToSelectList(selectedValue?.ToString());
    }
}
