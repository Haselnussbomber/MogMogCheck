using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Extensions;
using HaselCommon.Sheets;
using HaselCommon.Utils;
using ImGuiNET;
using MogMogCheck.Records;
using MogMogCheck.Tables;

namespace MogMogCheck.Windows;

public unsafe class MainWindow : Window
{
    private readonly ExtendedSpecialShop? _shop;
    private readonly Reward[]? _rewardsItems;
    private readonly RewardsTable? _rewardsTable;
    private readonly ExtendedItem? _tomestone;

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

        _shop = GetRow<ExtendedSpecialShop>(1769929);
        if (_shop == null)
        {
            OnClose();
            return;
        }

        _rewardsItems = _shop.Items
                .Where(row => row.ReceiveItemId1 != 0 && row.GiveItemId1 != 41306)
                .OrderBy(row => row.GiveItemId1)
                .ThenBy(row => row.SortKey)
                .Select((row, index) => new Reward(index, row))
                .ToArray();

        // clear old untracked items
        if (Plugin.Config.TrackedItems.RemoveAll((uint itemId, uint amount) => amount == 0 || !_rewardsItems.Any(entry => entry.ReceiveItems.Any(ri => ri.Item?.RowId == itemId))))
            Plugin.Config.Save();

        _rewardsTable = new(_rewardsItems);

        _tomestone = GetRow<ExtendedItem>(_rewardsItems![0].GiveItems[0].Item?.RowId ?? 0);

    }

    public override void OnClose()
    {
        Service.WindowManager.CloseWindow<MainWindow>();
    }

    public override bool DrawConditions()
        => Service.ClientState.IsLoggedIn
            && _shop != null
            && _rewardsItems != null
            && _rewardsTable?.TotalItems > 0
            && _tomestone?.RowId > 0;

    public override void Draw()
    {
        var scale = ImGuiHelpers.GlobalScale;

        Service.TextureManager.GetIcon(_tomestone!.Icon).Draw(32 * scale);

        new ImGuiContextMenu($"##Tomestone_ItemContextMenu{_tomestone.RowId}_Tooltip")
        {
            ImGuiContextMenu.CreateItemFinder(_tomestone.RowId),
            ImGuiContextMenu.CreateCopyItemName(_tomestone.RowId),
            ImGuiContextMenu.CreateItemSearch(_tomestone),
            ImGuiContextMenu.CreateOpenOnGarlandTools(_tomestone.RowId),
        }
        .Draw();

        ImGui.SameLine(45 * scale);
        ImGuiUtils.PushCursorY(6 * scale);

        var owned = InventoryManager.Instance()->GetInventoryItemCount(_tomestone.RowId);
        var needed = _shop!.Items.Aggregate(0u, (total, item) => total + (Plugin.Config.TrackedItems.TryGetValue((uint)item.ReceiveItemId1, out var amount) ? amount * item.GiveCount1 : 0));
        if (needed > owned)
        {
            var remaining = needed - owned;
            ImGui.TextUnformatted(t("Currency.InfoWithRemaining", owned, needed, remaining));
        }
        else
        {
            ImGui.TextUnformatted(t("Currency.Info", owned, needed));
        }

        _rewardsTable?.Draw((ImGui.GetTextLineHeight() + ImGui.GetStyle().ItemInnerSpacing.Y * 0.5f + ImGui.GetStyle().ItemSpacing.Y) * 2f - 1);
    }
}
