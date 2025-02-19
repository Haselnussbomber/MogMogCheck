using HaselCommon;
using HaselCommon.Gui.ImGuiTable;
using ImGuiNET;
using MogMogCheck.Config;
using MogMogCheck.Records;
using MogMogCheck.Services;

namespace MogMogCheck.Tables;

[RegisterSingleton, AutoConstruct]
public partial class ShopItemTable : Table<ShopItem>, IDisposable
{
    private readonly TrackColumn _trackColumn;
    private readonly RewardColumn _rewardColumn;
    private readonly RequiredItemColumn _requiredItemColumn;
    private readonly SpecialShopService _specialShopService;

    [AutoPostConstruct]
    private void Initialize()
    {
        Columns = [
            _trackColumn,
            _rewardColumn,
            _requiredItemColumn,
        ];
    }

    public override void LoadRows()
    {
        Rows = [.. _specialShopService.ShopItems];

        // Really not the best solution...
        var lineHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().CellPadding.Y * 2f;
        LineHeight = lineHeight;
        _trackColumn.Width = ImGui.GetFrameHeight() * (Service.Get<PluginConfig>().CheckboxMode ? 1 : 3);
    }

    // I should probably rework this...
    public void SetReloadPending()
    {
        RowsLoaded = false;
    }
}
