using HaselCommon.Services;
using HaselCommon.Utils;
using MogMogCheck.Caches;

namespace MogMogCheck.Extensions;

public static class ItemServiceExtensions
{
    extension(ItemService itemService)
    {
        public bool IsUnlockedOrCollected(ItemHandle item)
        {
            // Unlockables
            if (itemService.IsUnlocked(item))
                return true;

            // Equipable items for "All Classes"
            if (itemService.TryGetItem(item, out var itemRow)
                && itemRow.ClassJobCategory.RowId == 1
                && ServiceLocator.TryGetService<ItemQuantityService>(out var itemQuantityCache)
                && itemQuantityCache.Get(item) is { } itemQuantity
                && itemQuantity != 0)
            {
                return true;
            }

            return false;
        }
    }
}
