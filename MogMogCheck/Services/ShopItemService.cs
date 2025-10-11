using HaselCommon.Utils;
using MogMogCheck.Caches;

namespace MogMogCheck.Services;

[RegisterSingleton, AutoConstruct]
public partial class ShopItemService
{
    private readonly ItemQuantityCache _itemQuantityCache;

    public bool IsUnlockedOrCollected(ItemHandle item)
    {
        // Unlockables
        if (item.IsUnlocked)
            return true;

        // Equipable items for "All Classes"
        if (item.TryGetItem(out var itemRow)
            && itemRow.ClassJobCategory.RowId == 1
            && _itemQuantityCache.TryGetValue(item, out var itemQuantity)
            && itemQuantity != 0)
        {
            return true;
        }

        return false;
    }
}
