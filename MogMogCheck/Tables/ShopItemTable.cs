using Dalamud.Interface.Utility;
using Dalamud.Plugin;
using HaselCommon.Gui.ImGuiTable;
using MogMogCheck.Config;
using MogMogCheck.Records;
using MogMogCheck.Services;

namespace MogMogCheck.Tables;

[RegisterSingleton, AutoConstruct]
public partial class ShopItemTable : Table<ShopItem>
{
    private readonly TrackColumn _trackColumn;
    private readonly RewardColumn _rewardColumn;
    private readonly RequiredItemColumn _requiredItemColumn;
    private readonly SpecialShopService _specialShopService;
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly PluginConfig _pluginConfig;

    [AutoPostConstruct]
    private void Initialize()
    {
        Columns = [
            _trackColumn,
            _rewardColumn,
            _requiredItemColumn,
        ];

        _pluginInterface.UiBuilder.DefaultGlobalScaleChanged += OnGlobalScaleChanged;
    }

    public override void Dispose()
    {
        _pluginInterface.UiBuilder.DefaultGlobalScaleChanged -= OnGlobalScaleChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnGlobalScaleChanged()
    {
        UpdateColumnWidth();
    }

    private void UpdateColumnWidth()
    {
        _trackColumn.Width = ImGui.GetFrameHeight() / ImGuiHelpers.GlobalScale * (_pluginConfig.CheckboxMode ? 1 : 3);
    }

    public override float CalculateLineHeight()
    {
        return ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().CellPadding.Y * 2f;
    }

    public override void LoadRows()
    {
        Rows = [.. _specialShopService.ShopItems];
        UpdateColumnWidth();
    }

    // I should probably rework this...
    public void SetReloadPending()
    {
        RowsLoaded = false;
    }
}
