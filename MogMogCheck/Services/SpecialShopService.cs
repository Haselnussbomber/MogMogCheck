using System.Linq;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon;
using HaselCommon.Services;
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
    private bool _isDirty = true;

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
        if (manager == null || manager->IsOpenShop == 0)
            return;

        if (!_isDirty && CurrentSeason != null && CurrentSeason == manager->SeasonTarget)
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
        }
        else
        {
            CurrentShop = null;
            TomestoneItemIds = [];
            ShopItems = [];
        }

        if (Service.TryGet<ShopItemTable>(out var shopItemTable))
            shopItemTable.SetReloadPending();

        _isDirty = false;
    }

    public void SetIsDirty()
    {
        _isDirty = true;
    }
}
