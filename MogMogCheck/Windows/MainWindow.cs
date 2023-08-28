using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Sheets;
using HaselCommon.Utils;
using ImGuiNET;
using MogMogCheck.Records;
using MogMogCheck.Sheets;
using MogMogCheck.Tables;
using InstanceContentCSBonus = Lumina.Excel.GeneratedSheets.InstanceContentCSBonus;

namespace MogMogCheck.Windows;

public unsafe class MainWindow : Window
{
    private SpecialShop? _shop;
    private RewardsTable? _rewardsTable;
    private DutiesTable? _dutiesTable;

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
    }

    public override void Update()
    {
        _shop = GetRow<SpecialShop>(1769929);

        if (_rewardsTable == null && _shop != null)
        {
            var rewards = new List<Reward>();

            for (var i = 0u; i < _shop.Items!.Length; i++)
            {
                var row = _shop.Items![i];
                if (row.ItemId == 0)
                    continue;

                var rewardItem = GetRow<Item>((uint)row.ItemId);
                var requiredItem = GetRow<Item>((uint)row.RequiredItem);
                if (rewardItem == null || requiredItem == null)
                    continue;

                rewards.Add(new(i, rewardItem, row.StackSize, requiredItem, row.RequiredCount));
            }

            _rewardsTable = new(rewards);
        }

        if (_dutiesTable == null)
        {
            var duties = new List<Duty>();

            foreach (var row in GetSheet<InstanceContentCSBonus>()!)
            {
                if (row.Item.Row == 0)
                    continue;

                var instanceContent = GetRow<InstanceContent>(row.Instance.Row);
                if (instanceContent == null || instanceContent.ContentFinderCondition.Row == 0)
                    continue;

                duties.Add(new(instanceContent.ContentFinderCondition.Value!, GetRow<Item>(row.Item.Row)!, row.Unknown2, row.Unknown3));
            }

            _dutiesTable = new(duties);
        }
    }

    public override void OnClose()
    {
        Service.WindowManager.CloseWindow<MainWindow>();
    }

    public override bool DrawConditions()
    {
        return Service.ClientState.IsLoggedIn
            && _shop != null
            && _shop.Items.Length > 0
            && _dutiesTable != null;
    }

    public override void Draw()
    {
        var scale = ImGuiHelpers.GlobalScale;

        var tomestone = GetRow<Item>((uint)_shop!.Items[0].RequiredItem);
        if (tomestone == null)
            return;

        Service.TextureManager.GetIcon(tomestone.Icon).Draw(32 * scale);

        new ImGuiContextMenu($"##Tomestone_ItemContextMenu{tomestone.RowId}_Tooltip")
        {
            ImGuiContextMenu.CreateItemFinder(tomestone.RowId),
            ImGuiContextMenu.CreateCopyItemName(tomestone.RowId),
            ImGuiContextMenu.CreateItemSearch(tomestone.RowId),
            ImGuiContextMenu.CreateOpenOnGarlandTools(tomestone.RowId),
        }
        .Draw();

        ImGui.SameLine(45 * scale);
        ImGuiUtils.PushCursorY(6 * scale);

        var owned = InventoryManager.Instance()->GetInventoryItemCount((uint)_shop.Items[0].RequiredItem);
        var needed = _shop.Items.Aggregate(0u, (total, item) => total + (Plugin.Config.TrackedItems.TryGetValue((uint)item.ItemId, out var tracked) && tracked ? item.RequiredCount : 0));
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

        var textHeight = ImGui.CalcTextSize("").Y;
        var iconSize = (textHeight + ImGui.GetStyle().ItemInnerSpacing.Y) * 2f;
        var rowHeight = iconSize + ImGui.GetStyle().ItemSpacing.Y;
        _rewardsTable?.Draw(rowHeight);
    }

    public void DrawDutiesTab()
    {
        using var dutiesTab = ImRaii.TabItem(t("Tabs.Duties"));
        if (!dutiesTab.Success)
            return;

        _dutiesTable?.Draw(32 * ImGuiHelpers.GlobalScale + ImGui.GetStyle().ItemSpacing.Y);
    }
}
