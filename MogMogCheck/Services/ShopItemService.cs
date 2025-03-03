using HaselCommon.Services;
using HaselCommon.Utils;
using Lumina.Excel.Sheets;
using MogMogCheck.Caches;

namespace MogMogCheck.Services;

[RegisterSingleton, AutoConstruct]
public partial class ShopItemService
{
    private readonly ItemService _itemService;
    private readonly ItemQuantityCache _itemQuantityCache;

    public bool IsUnlockedOrCollected(ExcelRowId<Item> itemId)
    {
        // Unlockables
        if (_itemService.IsUnlockable(itemId) &&
            _itemService.IsUnlocked(itemId))
        {
            return true;
        }

        // Equipable items for "All Classes"
        if (itemId.TryGetRow(out var item) &&
            item.ClassJobCategory.RowId == 1 &&
            _itemQuantityCache.TryGetValue(itemId, out var itemQuantity) &&
            itemQuantity != 0)
        {
            return true;
        }

        return false;
    }
}
