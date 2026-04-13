using System.Numerics;
using HaselCommon.Gui;
using HaselCommon.Gui.ImGuiTable;
using HaselCommon.Services;
using MogMogCheck.Config;
using MogMogCheck.Records;

namespace MogMogCheck.Tables;

[RegisterSingleton, AutoConstruct]
public partial class TrackColumn : Column<ShopItem>
{
    private readonly PluginConfig _pluginConfig;
    private readonly TextService _textService;
    private readonly ItemService _itemService;

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
        var item = row.ReceiveItems[0].Item;

        // Ensure LineHeight???
        var pos = ImCursor.Position;
        ImGui.Dummy(new Vector2(ImGui.GetFrameHeightWithSpacing()));
        ImCursor.Position = pos;

        if (!_itemService.TryGetItem(item, out var itemRow))
            return;

        ImCursor.Y += MathF.Round(ImStyle.FramePadding.Y / 2f); // my cell padding

        ImGui.SetNextItemWidth(-1);

        if (_pluginConfig.CheckboxMode)
        {
            if (!_pluginConfig.TrackedItems.TryGetValue(item, out var savedAmount))
                savedAmount = 0;

            var isChecked = savedAmount > 0;
            if (ImGui.Checkbox("##Row"u8, ref isChecked))
            {
                if (isChecked)
                {
                    if (!_pluginConfig.TrackedItems.ContainsKey(item))
                        _pluginConfig.TrackedItems.Add(item, 1);
                    else
                        _pluginConfig.TrackedItems[item] = 1;
                }
                else
                {
                    _pluginConfig.TrackedItems.Remove(item);
                }

                _pluginConfig.Save();
            }

            if (ImGui.IsItemHovered() || ImGui.IsItemActive())
            {
                ImGui.BeginTooltip();
                ImGui.Text(_textService.Translate("Reward.AmountInput.Tooltip.ResultOnly", row.GiveItems[0].Amount));
                ImGui.EndTooltip();
            }
        }
        else
        {
            var canSell = !itemRow.IsUnique && !itemRow.IsUntradable && !itemRow.IsCollectable;
            var stackSize = canSell ? 999 : itemRow.StackSize;

            if (!_pluginConfig.TrackedItems.TryGetValue(item, out var savedAmount))
                savedAmount = 0;

            var inputAmount = (int)savedAmount;

            var changed = ImGui.DragInt("##Row"u8, ref inputAmount, 1, 0, (int)stackSize, $"%d / {stackSize}", ImGuiSliderFlags.AlwaysClamp);
            if (changed && savedAmount != inputAmount)
            {
                if (inputAmount > 0)
                {
                    if (!_pluginConfig.TrackedItems.ContainsKey(item))
                        _pluginConfig.TrackedItems.Add(item, (uint)inputAmount);
                    else
                        _pluginConfig.TrackedItems[item] = (uint)inputAmount;
                }
                else
                {
                    _pluginConfig.TrackedItems.Remove(item);
                }

                _pluginConfig.Save();
            }

            if (ImGui.IsItemHovered() || ImGui.IsItemActive())
            {
                ImGui.BeginTooltip();

                ImGui.Text(inputAmount <= 1
                    ? _textService.Translate("Reward.AmountInput.Tooltip.ResultOnly", inputAmount * row.GiveItems[0].Amount)
                    : _textService.Translate("Reward.AmountInput.Tooltip.Calculation", inputAmount, row.GiveItems[0].Amount, inputAmount * row.GiveItems[0].Amount));

                ImGui.EndTooltip();
            }
        }
    }
}
