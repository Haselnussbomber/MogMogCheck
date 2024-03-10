using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using HaselCommon.Extensions;
using HaselCommon.Sheets;
using HaselCommon.Utils;
using ImGuiNET;
using MogMogCheck.Records;
using MogMogCheck.Structs;
using MogMogCheck.Tables;

namespace MogMogCheck.Windows;

public unsafe class MainWindow : Window
{
    private ExtendedSpecialShop? _shop = null;
    private Reward[]? _rewardsItems = null;
    private RewardsTable? _rewardsTable = null;
    private uint? _season = null;
    private uint _shopId;
    private bool _isDirty;

    public Dictionary<uint, TrackedItem> TomestoneItems { get; } = [];

    public MainWindow() : base("MogMogCheck")
    {
        Namespace = "MogMogCheckMain";

        Size = new Vector2(570, 740);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(300, 200),
            MaximumSize = new Vector2(4069),
        };

        TitleBarButtons.Add(new()
        {
            Icon = Dalamud.Interface.FontAwesomeIcon.Cog,
            IconOffset = new(0, 1),
            ShowTooltip = () => ImGui.SetTooltip(t($"TitleBarButton.ToggleConfig.Tooltip.{(Service.WindowManager.IsWindowOpen<ConfigWindow>() ? "Close" : "Open")}Config")),
            Click = (button) => { Service.WindowManager.ToggleWindow<ConfigWindow>(); }
        });
    }

    public void MarkDirty() => _isDirty = true;

    public override void Update()
    {
        var manager = CSBonusManager.Instance();
        if (!_isDirty && _season != null && _season == manager->SeasonTarget)
            return;

        _season = manager->SeasonTarget;
        _shopId = _season == 0 ? 1770710u : 1769929;

        Service.PluginLog.Verbose($"Update: Season {_season}, Shop Id {_shopId}");

        _shop = GetRow<ExtendedSpecialShop>(_shopId);
        if (_shop == null)
        {
            OnClose();
            return;
        }

        var rewards = new List<Reward>();
        var lastGiveItemId = 0;
        var index = 0;
        foreach (var row in _shop.Items
            .Where(row => row.ReceiveItemId1 != 0 && row.GiveItemId1 != 0)
            .OrderBy(row => row.SortKey))
        {
            if (lastGiveItemId != 0 && lastGiveItemId != row.GiveItemId1 && Service.GetService<Configuration>().HidePreviousSeasons)
                break;

            rewards.Add(new Reward(index++, row));
            lastGiveItemId = row.GiveItemId1;
        }

        _rewardsItems = [.. rewards];

        // clear old untracked items
        if (Service.GetService<Configuration>().TrackedItems.RemoveAll((uint itemId, uint amount) => amount == 0 || !_rewardsItems.Any(entry => entry.ReceiveItems.Any(ri => ri.Item?.RowId == itemId))))
            Service.GetService<Configuration>().Save();

        _rewardsTable = new(_rewardsItems);

        TomestoneItems.Clear();
        foreach (var reward in _rewardsItems)
        {
            foreach (var (Item, Quantity) in reward.GiveItems)
            {
                if (Item == null) continue;
                if (TomestoneItems.ContainsKey(Item.RowId)) continue;
                TomestoneItems.Add(Item.RowId, new TrackedItem(Item, Quantity));
            }
        }

        _isDirty = false;
    }

    public override void OnClose()
    {
        Service.WindowManager.CloseWindow<MainWindow>();
    }

    public override bool DrawConditions()
        => Service.ClientState.IsLoggedIn
            && _season != null
            && _shop != null
            && _rewardsItems != null
            && _rewardsTable?.TotalItems > 0
            && TomestoneItems.Any();

    public override void PreDraw()
    {
        base.PreDraw();
    }

    public override void Draw()
    {
        var scale = ImGuiHelpers.GlobalScale;

        foreach (var (_, trackedItem) in TomestoneItems)
        {
            var (item, quantity) = (trackedItem.Item, trackedItem.Quantity);

            Service.TextureManager.GetIcon(item.Icon).Draw(32 * scale);

            new ImGuiContextMenu($"##Tomestone_ItemContextMenu{item.RowId}_Tooltip")
            {
                ImGuiContextMenu.CreateItemFinder(item.RowId),
                ImGuiContextMenu.CreateCopyItemName(item.RowId),
                ImGuiContextMenu.CreateItemSearch(item),
                ImGuiContextMenu.CreateOpenOnGarlandTools("item", item.RowId),
            }
            .Draw();

            ImGui.SameLine(45 * scale);
            ImGuiUtils.PushCursorY(6 * scale);

            var needed = _shop!.Items
                .Where(row => row.GiveItemId1 == item.RowId || row.GiveItemId2 == item.RowId || row.GiveItemId3 == item.RowId)
                .Aggregate(0u, (total, item) => total + (Service.GetService<Configuration>().TrackedItems.TryGetValue((uint)item.ReceiveItemId1, out var amount) ? amount * item.GiveCount1 : 0));

            if (needed > quantity)
            {
                var remaining = needed - quantity;
                ImGui.TextUnformatted(t("Currency.InfoWithRemaining", quantity, needed, remaining));
            }
            else
            {
                ImGui.TextUnformatted(t("Currency.Info", quantity, needed));
            }

        }

        _rewardsTable?.Draw((ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemInnerSpacing.Y * 0.5f + ImGui.GetStyle().ItemSpacing.Y) * 2f - 1);
    }
}
