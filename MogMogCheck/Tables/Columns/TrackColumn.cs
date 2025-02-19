using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.ImGuiTable;
using HaselCommon.Services;
using ImGuiNET;
using Lumina.Excel.Sheets;
using MogMogCheck.Config;
using MogMogCheck.Records;

namespace MogMogCheck.Tables;

[RegisterSingleton, AutoConstruct]
public partial class TrackColumn : Column<ShopItem>
{
    private readonly PluginConfig _pluginConfig;
    private readonly ExcelService _excelService;
    private readonly TextService _textService;

    [AutoPostConstruct]
    private void Initialize()
    {
        AutoLabel = false;

        Flags &= ~ImGuiTableColumnFlags.WidthStretch;
        Flags |= ImGuiTableColumnFlags.WidthFixed;
        // Width is set by ShopItemTable.LoadRows when it's drawn
    }

    public override int Compare(ShopItem lhs, ShopItem rhs)
    {
        return lhs.Index.CompareTo(rhs.Index);
    }

    public override void DrawColumn(ShopItem row)
    {
        var itemId = row.ReceiveItems[0].ItemId;

        // Ensure LineHeight???
        var pos = ImGui.GetCursorPos();
        ImGui.Dummy(new Vector2(ImGui.GetFrameHeightWithSpacing()));
        ImGui.SetCursorPos(pos);

        if (!_excelService.TryGetRow<Item>(itemId, out var itemRow))
            return;

        ImGuiUtils.PushCursorY(MathF.Round(ImGui.GetStyle().FramePadding.Y / 2f)); // my cell padding

        ImGui.SetNextItemWidth(-1);

        if (_pluginConfig.CheckboxMode)
        {
            if (!_pluginConfig.TrackedItems.TryGetValue(itemId, out var savedAmount))
                savedAmount = 0;

            var isChecked = savedAmount > 0;
            if (ImGui.Checkbox($"##Row{row.Index}", ref isChecked))
            {
                if (isChecked)
                {
                    if (!_pluginConfig.TrackedItems.ContainsKey(itemId))
                        _pluginConfig.TrackedItems.Add(itemId, 1);
                    else
                        _pluginConfig.TrackedItems[itemId] = 1;
                }
                else
                {
                    _pluginConfig.TrackedItems.Remove(itemId);
                }

                _pluginConfig.Save();
            }

            if (ImGui.IsItemHovered() || ImGui.IsItemActive())
            {
                ImGui.BeginTooltip();
                ImGui.TextUnformatted(_textService.Translate("Reward.AmountInput.Tooltip.ResultOnly", row.GiveItems[0].Quantity));
                ImGui.EndTooltip();
            }
        }
        else
        {
            var canSell = !itemRow.IsUnique && !itemRow.IsUntradable && !itemRow.IsCollectable;
            var stackSize = canSell ? 999 : itemRow.StackSize;

            if (!_pluginConfig.TrackedItems.TryGetValue(itemId, out var savedAmount))
                savedAmount = 0;

            var inputAmount = (int)savedAmount;

            var changed = ImGui.DragInt($"##Row{row.Index}", ref inputAmount, 1, 0, (int)stackSize, $"%d / {stackSize}", ImGuiSliderFlags.AlwaysClamp);
            if (changed && savedAmount != inputAmount)
            {
                if (inputAmount > 0)
                {
                    if (!_pluginConfig.TrackedItems.ContainsKey(itemId))
                        _pluginConfig.TrackedItems.Add(itemId, (uint)inputAmount);
                    else
                        _pluginConfig.TrackedItems[itemId] = (uint)inputAmount;
                }
                else
                {
                    _pluginConfig.TrackedItems.Remove(itemId);
                }

                _pluginConfig.Save();
            }

            if (ImGui.IsItemHovered() || ImGui.IsItemActive())
            {
                ImGui.BeginTooltip();

                ImGui.TextUnformatted(inputAmount <= 1
                    ? _textService.Translate("Reward.AmountInput.Tooltip.ResultOnly", inputAmount * row.GiveItems[0].Quantity)
                    : _textService.Translate("Reward.AmountInput.Tooltip.Calculation", inputAmount, row.GiveItems[0].Quantity, inputAmount * row.GiveItems[0].Quantity));

                ImGui.EndTooltip();
            }
        }
    }
}
