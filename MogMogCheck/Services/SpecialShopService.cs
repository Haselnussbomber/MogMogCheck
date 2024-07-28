using System.Collections.Generic;
using System.Linq;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Services;
using Microsoft.Extensions.Logging;
using MogMogCheck.Config;
using MogMogCheck.Records;
using Item = Lumina.Excel.GeneratedSheets.Item;
using SpecialShop = Lumina.Excel.GeneratedSheets2.SpecialShop;

namespace MogMogCheck.Services;

public class SpecialShopService(
    ILogger<SpecialShopService> Logger,
    ExcelService ExcelService,
    PluginConfig PluginConfig)
{
    public bool IsDirty { get; set; }
    public uint? CurrentSeason { get; private set; }
    public uint CurrentShopId { get; private set; }
    public SpecialShop? CurrentShop { get; private set; }
    public Reward[] RewardsItems { get; private set; } = [];
    public Dictionary<uint, Item> TomestoneItems { get; private set; } = [];

    public unsafe bool Update()
    {
        var manager = CSBonusManager.Instance();
        if (!IsDirty && CurrentSeason != null && CurrentSeason == manager->SeasonTarget)
            return false;

        CurrentSeason = manager->SeasonTarget;
        CurrentShopId = CurrentSeason == 0 ? 1770710u : 1769929;

        Logger.LogDebug("Update: Season {season}, Shop Id {shopId}", CurrentSeason, CurrentShopId);

        CurrentShop = ExcelService.GetRow<SpecialShop>(CurrentShopId);
        if (CurrentShop == null)
            return true;

        var rewards = new List<Reward>();
        var lastGiveItemId = 0;
        var index = 0;
        foreach (var row in CurrentShop.Item
            .Where(row => row.Item[0].Row != 0 && row.ItemCost[0] != 0)
            .OrderBy(row => row.Unknown1[5]))
        {
            if (lastGiveItemId != 0 && lastGiveItemId != row.ItemCost[0] && PluginConfig.HidePreviousSeasons)
                break;

            rewards.Add(new Reward(index++, row, ExcelService));
            lastGiveItemId = row.ItemCost[0];
        }

        RewardsItems = [.. rewards];

        TomestoneItems.Clear();
        foreach (var reward in RewardsItems)
        {
            foreach (var (item, quantity) in reward.GiveItems)
            {
                if (item == null || item.RowId == 0)
                    continue;

                TomestoneItems.TryAdd(item.RowId, item);
            }
        }

        IsDirty = false;
        return true;
    }
}
