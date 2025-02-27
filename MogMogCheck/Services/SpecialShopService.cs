using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon;
using HaselCommon.Services;
using HaselCommon.Utils;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Logging;
using MogMogCheck.Config;
using MogMogCheck.Records;
using MogMogCheck.Tables;

namespace MogMogCheck.Services;

[RegisterSingleton, AutoConstruct]
public partial class SpecialShopService : IDisposable
{
    private readonly ILogger<SpecialShopService> _logger;
    private readonly ExcelService _excelService;
    private readonly PluginConfig _pluginConfig;
    private readonly IFramework _framework;
    private uint? _seasonTarget;

    public bool HasData { get; private set; }
    public ExcelRowId<Item> TomestoneItemId { get; private set; }
    public ShopItem[] ShopItems { get; private set; } = [];

    [AutoPostConstruct]
    private void Initialize()
    {
        _framework.Update += Update;
    }

    public void Dispose()
    {
        _framework.Update -= Update;
    }

    private unsafe void Update(IFramework framework)
    {
        var manager = CSBonusManager.Instance();
        if (manager == null || manager->IsOpenShop == 0)
        {
            if (HasData)
                HasData = false;
            return;
        }

        if (_seasonTarget != null && _seasonTarget == manager->SeasonTarget)
            return;

        if (!_excelService.TryGetRow<CSBonusSeason>(manager->State, out var seasonRow) ||
            !_excelService.TryGetRow<SpecialShop>(manager->SeasonTarget == 0 ? 1770710u : 1769929, out var currentShop))
        {
            if (HasData)
                HasData = false;
            return;
        }

        _seasonTarget = manager->SeasonTarget;
        TomestoneItemId = seasonRow.Item.RowId;

        ShopItems = currentShop.Item
            .Select(item =>
            {
                var receiveItems = item.ReceiveItems
                    .Where(ri => ri.ReceiveCount > 0 && ri.Item.RowId != 0 && ri.Item.IsValid)
                    .Select(ri => new ItemEntry(ri.Item.RowId, ri.ReceiveCount))
                    .ToArray();

                if (receiveItems.Length == 0)
                    return default;

                var giveItems = item.ItemCosts
                    .Where(gi => gi.ItemCost.RowId != 0 && gi.ItemCost.IsValid)
                    .Select(gi => new ItemEntry(gi.ItemCost.RowId, gi.CurrencyCost))
                    .ToArray();

                if (giveItems.Length == 0)
                    return default;

                return new ShopItem(item.Unknown1[5], receiveItems, giveItems); // while I support multiple items here, it's not supported in the table
            })
            .Where(item => item != default)
            .OrderBy(item => item.Index)
            .ToArray();

        _logger.LogDebug("Update: SeasonTarget {seasonTarget}, Shop Id {shopId}", _seasonTarget, currentShop.RowId);

        if (Service.TryGet<ShopItemTable>(out var shopItemTable))
            shopItemTable.SetReloadPending();

        HasData = true;
    }
}
