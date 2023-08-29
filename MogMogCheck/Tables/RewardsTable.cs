using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Raii;
using Dalamud.Interface.Table;
using HaselCommon.Utils;
using ImGuiNET;
using MogMogCheck.Records;

namespace MogMogCheck.Tables;

public class RewardsTable : Table<Reward>
{
    private static readonly TrackColumn _trackColumn = new()
    {
        Label = t("Table.Rewards.Header.Track")
    };

    private static readonly RewardColumn _rewardColumn = new()
    {
        Label = t("Table.Rewards.Header.Reward"),
        Flags = ImGuiTableColumnFlags.WidthStretch
    };

    private static readonly RequiredItemColumn _requiredItemColumn = new()
    {
        Label = t("Table.Rewards.Header.RequiredItem")
    };

    public RewardsTable(ICollection<Reward> items) : base("Rewards", items, _trackColumn, _rewardColumn, _requiredItemColumn)
    {
        Flags &= ~ImGuiTableFlags.Borders;
    }

    private sealed class TrackColumn : Column<Reward>
    {
        public override float Width
            => 28 * ImGuiHelpers.GlobalScale;

        public override bool DrawFilter()
        {
            return false;
        }

        public override int Compare(Reward lhs, Reward rhs)
            => lhs.Index.CompareTo(rhs.Index);

        public override void DrawColumn(Reward row, int idx)
        {
            var scale = ImGuiHelpers.GlobalScale;
            var itemInnerSpacing = ImGui.GetStyle().ItemInnerSpacing;
            var rowHeight = (ImGui.GetTextLineHeight() + itemInnerSpacing.Y) * 2f;
            var paddingY = (rowHeight - ImGui.GetFrameHeight()) * 0.5f;

            var itemId = row.ReceiveItems[0].Item!.RowId;

            if (!Plugin.Config.TrackedItems.TryGetValue(itemId, out var tracked))
                Plugin.Config.TrackedItems.Add(itemId, tracked = false);

            ImGuiUtils.PushCursor(ImGui.GetStyle().ItemInnerSpacing.X * scale, paddingY);

            if (ImGui.Checkbox($"##Row{idx}", ref tracked))
            {
                if (tracked)
                {
                    if (!Plugin.Config.TrackedItems.ContainsKey(itemId))
                        Plugin.Config.TrackedItems.Add(itemId, tracked);
                    else
                        Plugin.Config.TrackedItems[itemId] = tracked;
                }
                else
                {
                    Plugin.Config.TrackedItems.Remove(itemId);
                }

                Plugin.Config.Save();
            }
        }
    }

    private sealed class RewardColumn : ColumnString<Reward>
    {
        public override string ToName(Reward reward)
            => GetItemName(reward.ReceiveItems[0].Item!.RowId);

        public override void DrawColumn(Reward row, int idx)
        {
            var scale = ImGuiHelpers.GlobalScale;
            var itemSpacing = ImGui.GetStyle().ItemSpacing;
            var itemInnerSpacing = ImGui.GetStyle().ItemInnerSpacing;

            var textHeight = ImGui.GetTextLineHeight();
            var rowHeight = (textHeight + itemInnerSpacing.Y) * 2f;
            var iconSize = (textHeight + itemInnerSpacing.Y) * 1.5f;
            var textOffsetX = iconSize + itemSpacing.X;

            // TODO: add support for item 2 (see: PLD shields)
            var item = row.ReceiveItems[0].Item!;
            var quantity = row.ReceiveItems[0].Quantity;

            var cursor = ImGui.GetCursorPos();
            ImGuiUtils.PushCursorY((rowHeight - iconSize + itemInnerSpacing.Y) * 0.5f);
            Service.TextureManager.GetIcon(item.Icon).Draw(iconSize);

            new ImGuiContextMenu($"##{idx}_ItemContextMenu{item.RowId}_IconTooltip")
            {
                ImGuiContextMenu.CreateItemFinder(item.RowId),
                ImGuiContextMenu.CreateCopyItemName(item.RowId),
                ImGuiContextMenu.CreateItemSearch(item.RowId),
                ImGuiContextMenu.CreateOpenOnGarlandTools(item.RowId),
            }
            .Draw();

            if (item.IsUnlockable && item.HasAcquired)
            {
                ImGui.SameLine(1, 0);

                var tex = Service.TextureProvider.GetTextureFromGame("ui/uld/RecipeNoteBook_hr1.tex");
                if (tex != null)
                {
                    var pos = ImGui.GetWindowPos() + ImGui.GetCursorPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY()) + new Vector2(iconSize / 2.5f + 4 * scale);
                    ImGui.GetWindowDrawList().AddImage(tex.ImGuiHandle, pos, pos + new Vector2(iconSize / 1.5f), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
                }
            }

            ImGui.SameLine(textOffsetX, 0);
            ImGui.SetCursorPosY(cursor.Y);
            ImGui.Selectable($"##{idx}_Item{item.RowId}_Selectable", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, rowHeight - itemSpacing.Y));

            new ImGuiContextMenu($"##{idx}_ItemContextMenu{item.RowId}_Tooltip")
            {
                ImGuiContextMenu.CreateItemFinder(item.RowId),
                ImGuiContextMenu.CreateCopyItemName(item.RowId),
                ImGuiContextMenu.CreateItemSearch(item.RowId),
                ImGuiContextMenu.CreateOpenOnGarlandTools(item.RowId),
            }
            .Draw();

            ImGui.SameLine(textOffsetX, 0);
            ImGui.SetCursorPosY(cursor.Y);
            ImGuiUtils.PushCursorY(itemInnerSpacing.Y * 0.5f * scale);
            using (ImRaii.PushColor(ImGuiCol.Text, (uint)Colors.GetItemRarityColor(item.Rarity)))
                ImGui.TextUnformatted($"{(quantity > 1 ? quantity.ToString() + "x " : "")}{GetItemName(item.RowId)}");

            ImGui.SameLine(textOffsetX, 0);
            ImGui.SetCursorPosY(cursor.Y + textHeight);
            using (ImRaii.PushColor(ImGuiCol.Text, (uint)Colors.Grey))
                ImGui.TextUnformatted($"{item.ItemUICategory.Value?.Name}");

            if (row.RequiredQuest != null)
            {
                var regionWidth = ImGui.GetContentRegionAvail().X;
                ImGui.SameLine(regionWidth - ImGuiUtils.GetIconSize(FontAwesomeIcon.InfoCircle).X - itemSpacing.X);
                ImGuiUtils.PushCursorY(itemInnerSpacing.Y * 0.5f * scale);
                ImGuiUtils.Icon(FontAwesomeIcon.InfoCircle, 0x7FFFFFFF);
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(t("Reward.RequiredQuest.Tooltip", row.RequiredQuest.Name));
                }
            }
        }
    }

    private sealed class RequiredItemColumn : Column<Reward>
    {
        public override float Width
            => 130;

        public override int Compare(Reward lhs, Reward rhs)
            => lhs.GiveItems[0].Quantity.CompareTo(rhs.GiveItems[0].Quantity);

        public override void DrawColumn(Reward row, int _)
        {
            var itemSpacing = ImGui.GetStyle().ItemSpacing;
            var itemInnerSpacing = ImGui.GetStyle().ItemInnerSpacing;
            var textHeight = ImGui.GetTextLineHeight();
            var rowHeight = (textHeight + itemInnerSpacing.Y) * 2f;
            var iconSize = (textHeight + itemInnerSpacing.Y) * 1.5f;
            var paddingY = (rowHeight - iconSize) * 0.5f;

            // TODO: add support for item 2 and 3
            var item = row.GiveItems[0].Item!;
            var quantity = row.GiveItems[0].Quantity;

            ImGuiUtils.PushCursorY(paddingY);
            Service.TextureManager.GetIcon(item.Icon).Draw(iconSize);

            new ImGuiContextMenu($"##{item.RowId}_ItemContextMenu{item.RowId}_Tooltip")
            {
                ImGuiContextMenu.CreateItemFinder(item.RowId),
                ImGuiContextMenu.CreateCopyItemName(item.RowId),
                ImGuiContextMenu.CreateItemSearch(item.RowId),
                ImGuiContextMenu.CreateOpenOnGarlandTools(item.RowId),
            }
            .Draw();

            ImGui.SameLine(iconSize + itemSpacing.X);
            ImGuiUtils.PushCursorY(paddingY);
            ImGui.TextUnformatted(t("CurrencyReward.Normal", quantity));
        }
    }
}
