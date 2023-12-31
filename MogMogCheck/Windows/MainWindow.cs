using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Sheets;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using MogMogCheck.Records;
using MogMogCheck.Tables;

namespace MogMogCheck.Windows;

public unsafe class MainWindow : Window
{
    private readonly ExtendedSpecialShop? _shop;
    private readonly RewardsTable? _rewardsTable;
    private readonly DutiesTable? _dutiesTable;

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

        _shop = GetRow<ExtendedSpecialShop>(1769929);
        if (_shop == null)
            return;

        _rewardsTable ??= new(
            _shop.Items
                .Where(row => row.ReceiveItemId1 != 0)
                .OrderBy(row => row.SortKey)
                .Select((row, index) => new Reward(index, row))
                .ToArray());

        _dutiesTable ??= new(
            GetSheet<InstanceContentCSBonus>()
                .Where(row => row.Item.Row != 0)
                .Select(row => new Duty(row))
                .Where(row => row.RewardItem != null && row.ContentFinderCondition != null && row.ContentFinderCondition.RowId != 0)
                .ToArray());
    }

    public override void OnClose()
    {
        Service.WindowManager.CloseWindow<MainWindow>();
    }

    public override bool DrawConditions()
    {
        return Service.ClientState.IsLoggedIn
            && _shop != null
            && _rewardsTable != null
            && _rewardsTable.TotalItems > 0
            && _dutiesTable != null;
    }

    public override void Draw()
    {
        var scale = ImGuiHelpers.GlobalScale;

        var tomestone = GetRow<ExtendedItem>((uint)_shop!.Items[0].GiveItemId1);
        if (tomestone == null)
            return;

        Service.TextureManager.GetIcon(tomestone.Icon).Draw(32 * scale);

        new ImGuiContextMenu($"##Tomestone_ItemContextMenu{tomestone.RowId}_Tooltip")
        {
            ImGuiContextMenu.CreateItemFinder(tomestone.RowId),
            ImGuiContextMenu.CreateCopyItemName(tomestone.RowId),
            ImGuiContextMenu.CreateItemSearch(tomestone),
            ImGuiContextMenu.CreateOpenOnGarlandTools(tomestone.RowId),
        }
        .Draw();

        ImGui.SameLine(45 * scale);
        ImGuiUtils.PushCursorY(6 * scale);

        var owned = InventoryManager.Instance()->GetInventoryItemCount((uint)_shop.Items[0].GiveItemId1);
        var needed = _shop.Items.Aggregate(0u, (total, item) => total + (Plugin.Config.TrackedItems.TryGetValue((uint)item.ReceiveItemId1, out var tracked) && tracked ? item.GiveCount1 : 0));
        if (needed > owned)
        {
            var remaining = needed - owned;
            ImGui.TextUnformatted(t("Currency.InfoWithRemaining", owned, needed, remaining));
        }
        else
        {
            ImGui.TextUnformatted(t("Currency.Info", owned, needed));
        }

        using var tabs = ImRaii.TabBar("##Tabs");
        DrawRewardsTab();
        DrawDutiesTab();
    }

    public void DrawRewardsTab()
    {
        using var rewardsTab = ImRaii.TabItem(t("Tabs.Rewards"));
        if (!rewardsTab.Success)
            return;

        _rewardsTable?.Draw((ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemInnerSpacing.Y * 0.5f + ImGui.GetStyle().ItemSpacing.Y) * 2f - 1);
    }

    public void DrawDutiesTab()
    {
        using var dutiesTab = ImRaii.TabItem(t("Tabs.Duties"));
        if (!dutiesTab.Success)
            return;

        _dutiesTable?.Draw(32 * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.Y);
    }
}
