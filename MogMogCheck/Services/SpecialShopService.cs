using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon;
using HaselCommon.Extensions.Collections;
using HaselCommon.Services;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.DependencyInjection;
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
    private readonly IFramework _framework;

    public uint? CurrentSeason { get; private set; }
    public uint CurrentShopId { get; private set; }
    public SpecialShop? CurrentShop { get; private set; }
    public ShopItem[] ShopItems { get; private set; } = [];
    public uint[] TomestoneItemIds { get; private set; } = [];

    [AutoPostConstruct]
    private void Initialize()
    {
        _framework.Update += Update;
    }

    public void Dispose()
    {
        _framework.Update -= Update;
        GC.SuppressFinalize(this);
    }

    private unsafe void Update(IFramework framework)
    {
        var manager = CSBonusManager.Instance();
        if (CurrentSeason != null && CurrentSeason == manager->SeasonTarget)
            return;

        CurrentSeason = manager->SeasonTarget;
        CurrentShopId = CurrentSeason == 0 ? 1770710u : 1769929;

        _logger.LogDebug("Update: Season {season}, Shop Id {shopId}", CurrentSeason, CurrentShopId);

        if (_excelService.TryGetRow<SpecialShop>(CurrentShopId, out var currentShop))
        {
            CurrentShop = currentShop;

            TomestoneItemIds = currentShop.Item
                .Select(item => item.ItemCosts[0].ItemCost.RowId)
                .Where(itemId => itemId != 0)
                .Distinct()
                .ToArray();

            ShopItems = currentShop.Item
                .Select((item, index) =>
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

                    return new ShopItem(index, receiveItems, giveItems);
                })
                .Where(item => item != default)
                .ToArray();

            // clear old untracked items
            var pluginConfig = Service.Provider?.GetService<PluginConfig>();
            if (pluginConfig != null && pluginConfig.TrackedItems.RemoveAll((uint itemId, uint amount) => amount == 0 || !ShopItems.Any(entry => entry.ReceiveItems.Any(ri => ri.ItemId == itemId))))
                pluginConfig.Save();
        }
        else
        {
            CurrentShop = null;
            TomestoneItemIds = [];
            ShopItems = [];
        }

        Service.Provider?.GetService<ShopItemTable>()?.SetReloadPending();
    }
}
