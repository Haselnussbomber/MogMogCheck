using HaselCommon;
using HaselCommon.Services;
using HaselCommon.Utils;
using Lumina.Excel.Sheets;
using MogMogCheck.Caches;

namespace MogMogCheck.Extensions;

public static class ExcelRowIdExtensions
{
    public static bool IsUnlockedOrCollected(this ExcelRowId<Item> itemId)
    {
        if (!Service.TryGet<ItemService>(out var itemService) || !Service.TryGet<ItemQuantityCache>(out var itemQuantityCache))
            return false;

        if (itemService.IsUnlockable(itemId) && itemService.IsUnlocked(itemId))
            return true;

        if (itemId.TryGetRow(out var item) && item.ClassJobCategory.RowId == 1 && itemQuantityCache.TryGetValue(itemId, out var itemQuantity) && itemQuantity != 0)
            return true;

        return false;
    }
}
