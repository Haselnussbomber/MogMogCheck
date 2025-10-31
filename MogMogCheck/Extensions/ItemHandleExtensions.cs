using HaselCommon.Services;
using HaselCommon.Utils;
using MogMogCheck.Caches;

namespace MogMogCheck.Extensions;

public static class ItemHandleExtensions
{
    public static bool IsUnlockedOrCollected(this ItemHandle item)
    {
        // Unlockables
        if (item.IsUnlocked)
            return true;

        // Equipable items for "All Classes"
        if (item.TryGetItem(out var itemRow)
            && itemRow.ClassJobCategory.RowId == 1
            && ServiceLocator.TryGetService<ItemQuantityCache>(out var itemQuantityCache)
            && itemQuantityCache.TryGetValue(item, out var itemQuantity)
            && itemQuantity != 0)
        {
            return true;
        }

        return false;
    }
}
