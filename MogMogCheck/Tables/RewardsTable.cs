using System.Collections.Generic;
using System.Numerics;
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
            => 28;

        public override bool DrawFilter()
        {
            return false;
        }

        public override int Compare(Reward lhs, Reward rhs)
            => lhs.Index.CompareTo(rhs.Index);

        public override void DrawColumn(Reward reward, int idx)
        {
            var scale = ImGui.GetIO().FontGlobalScale;
            var itemId = reward.Item.RowId;

            if (!Plugin.Config.TrackedItems.TryGetValue(itemId, out var tracked))
                Plugin.Config.TrackedItems.Add(itemId, tracked = false);

            ImGuiUtils.PushCursor(ImGui.GetStyle().ItemInnerSpacing.X, 6 * scale);

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
            => GetItemName(reward.Item.RowId);

        public override void DrawColumn(Reward reward, int idx)
        {
            var scale = ImGui.GetIO().FontGlobalScale;
            var item = reward.Item;
            var stackSize = reward.StackSize;

            Service.TextureManager.GetIcon(item.Icon).Draw(32 * scale);

            if (item.IsUnlockable && item.HasAcquired)
            {
                ImGui.SameLine(18 * scale, 0);
                ImGuiUtils.PushCursorY(16 * scale);

                var tex = Service.TextureProvider.GetTextureFromGame("ui/uld/RecipeNoteBook_hr1.tex");
                if (tex != null)
                {
                    var pos = ImGui.GetWindowPos() + ImGui.GetCursorPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
                    ImGui.GetWindowDrawList().AddImage(tex.ImGuiHandle, pos + Vector2.Zero, pos + new Vector2(24 * scale), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
                }
            }

            ImGui.SameLine(32 * scale + ImGui.GetStyle().ItemSpacing.X, 0);
            ImGui.Selectable($"##{idx}_Item{item.RowId}_Selectable", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 32 * scale));

            new ImGuiContextMenu($"##{idx}_ItemContextMenu{item.RowId}_Tooltip")
        {
            ImGuiContextMenu.CreateItemFinder(item.RowId),
            ImGuiContextMenu.CreateCopyItemName(item.RowId),
            ImGuiContextMenu.CreateItemSearch(item.RowId),
            ImGuiContextMenu.CreateOpenOnGarlandTools(item.RowId),
        }
            .Draw();

            var name = $"{(stackSize > 1 ? stackSize.ToString() + "x " : "")}{GetItemName(item.RowId)}";
            ImGui.SameLine(32 * scale + ImGui.GetStyle().ItemSpacing.X, 0);
            ImGuiUtils.PushCursorY(-1 * scale);
            using (ImRaii.PushColor(ImGuiCol.Text, (uint)Colors.GetItemRarityColor(item.Rarity)))
                ImGui.TextUnformatted(name);

            ImGui.SameLine(32 * scale + ImGui.GetStyle().ItemSpacing.X, 0);
            ImGuiUtils.PushCursorY(ImGui.GetFrameHeight() - 9 * scale);
            using (ImRaii.PushColor(ImGuiCol.Text, (uint)Colors.Grey))
                ImGui.TextUnformatted($"{item.ItemUICategory.Value?.Name}");
        }
    }

    private sealed class RequiredItemColumn : Column<Reward>
    {
        public override float Width
            => 130;

        public override int Compare(Reward lhs, Reward rhs)
            => lhs.RequiredCount.CompareTo(rhs.RequiredCount);

        public override void DrawColumn(Reward reward, int _)
        {
            var scale = ImGui.GetIO().FontGlobalScale;

            Service.TextureManager.GetIcon(reward.RequiredItem.Icon).Draw(32 * scale);

            new ImGuiContextMenu($"##{reward.Item.RowId}_ItemContextMenu{reward.RequiredItem.RowId}_Tooltip")
            {
                ImGuiContextMenu.CreateItemFinder(reward.RequiredItem.RowId),
                ImGuiContextMenu.CreateCopyItemName(reward.RequiredItem.RowId),
                ImGuiContextMenu.CreateItemSearch(reward.RequiredItem.RowId),
                ImGuiContextMenu.CreateOpenOnGarlandTools(reward.RequiredItem.RowId),
            }
            .Draw();

            ImGui.SameLine(32 * scale + ImGui.GetStyle().ItemSpacing.X);
            ImGuiUtils.PushCursorY(6 * scale);
            ImGui.TextUnformatted(t("CurrencyReward.Normal", reward.RequiredCount));
        }
    }
}
