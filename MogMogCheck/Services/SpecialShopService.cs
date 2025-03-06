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
    public uint ShopId { get; private set; }
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
        if (manager == null || !manager->EventInfo.IsOpenShop)
        {
            Reset();
            return;
        }

        if (_seasonTarget != null && _seasonTarget == manager->EventInfo.SeasonTarget)
            return;

        if (!_excelService.TryGetRow<CSBonusSeason>(manager->EventInfo.Season, out var seasonRow) ||
            !_excelService.TryGetRow<SpecialShop>(ShopId = manager->EventInfo.Season == 0 ? 1770710u : 1769929, out var currentShop))
        {
            Reset();
            return;
        }

        _seasonTarget = manager->EventInfo.SeasonTarget;
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

    private unsafe void Reset()
    {
        if (HasData)
            HasData = false;

        if (_seasonTarget != null)
            _seasonTarget = null;

        if (ShopId != 0)
            ShopId = 0;

        if (TomestoneItemId != 0)
            TomestoneItemId = 0;

        if (ShopItems.Length > 0)
            ShopItems = [];
    }
}
