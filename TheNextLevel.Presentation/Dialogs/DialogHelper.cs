using TheNextLevel.Application.DTOs;
using TheNextLevel.Core.DTOs;

namespace TheNextLevel.Presentation.Dialogs;

internal static class DialogHelper
{
    public static async Task<List<IItemDto>> LoadAllAsync<T>(
        Func<int, int, Task<PagedResult<T>>> listAsync,
        int pageSize) where T : IItemDto
    {
        var result = await listAsync(0, pageSize);
        var totalCount = result?.TotalCount ?? 0;
        var items = result?.Items.Cast<IItemDto>().ToList() ?? [];

        if (totalCount > pageSize)
        {
            var rest = await listAsync(pageSize, totalCount - pageSize);
            if (rest?.Items is not null)
                items.AddRange(rest.Items.Cast<IItemDto>());
        }

        return items;
    }
}
