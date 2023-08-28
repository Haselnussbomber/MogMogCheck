using System.Collections.Generic;
using Dalamud.Interface.Table;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using HaselCommon.Utils;
using ImGuiNET;
using MogMogCheck.Records;

namespace MogMogCheck.Tables;

public class DutiesTable : Table<Duty>
{
    private static readonly DutyColumn _dutyColumn = new()
    {
        Label = t("Table.Duties.Header.Duty"),
        Flags = ImGuiTableColumnFlags.WidthStretch
    };

    private static readonly RewardColumn _rewardColumn = new()
    {
        Label = t("Table.Duties.Header.Reward")
    };

    public DutiesTable(ICollection<Duty> items) : base("Duties", items, _dutyColumn, _rewardColumn)
    {
        Flags &= ~ImGuiTableFlags.Borders;
    }

    private sealed class DutyColumn : ColumnString<Duty>
    {
        public override string ToName(Duty item)
            => item.ContentFinderCondition.Name.RawString;

        public override unsafe void DrawColumn(Duty item, int _)
        {
            var scale = ImGui.GetIO().FontGlobalScale;

            var clicked = ImGui.Selectable($"##Duty{item.ContentFinderCondition.RowId}", false, ImGuiSelectableFlags.None, new(ImGui.GetContentRegionAvail().X, 32 * scale));
            /*
            if (ImGui.IsItemHovered() && !ImGui.IsKeyDown(ImGuiKey.LeftAlt))
            {
                var icon = Service.TextureProvider.GetIcon(item.ContentFinderCondition.Image);
                if (icon != null)
                {
                    using var tooltip = ImRaii.Tooltip();
                    ImGui.Image(icon.ImGuiHandle, new Vector2(1123, 360) * 0.5f);
                    ImGuiHelpers.CenteredText(ToName(item));
                }
            }
            */

            ImGui.SameLine(1, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGuiUtils.PushCursorY(6 * scale);
            ImGui.TextUnformatted(ToName(item));
            if (clicked)
            {
                AgentContentsFinder.Instance()->OpenRegularDuty(item.ContentFinderCondition.RowId);
            }
        }
    }

    private sealed class RewardColumn : Column<Duty>
    {
        public override float Width
            => 130;

        public override int Compare(Duty lhs, Duty rhs)
            => lhs.RewardItemCount.CompareTo(rhs.RewardItemCount);

        public override void DrawColumn(Duty item, int _)
        {
            var scale = ImGui.GetIO().FontGlobalScale;

            Service.TextureManager.GetIcon(item.RewardItem.Icon).Draw(32 * scale);

            new ImGuiContextMenu($"##{item.ContentFinderCondition.RowId}_ItemContextMenu{item.RewardItem.RowId}_Tooltip")
            {
                ImGuiContextMenu.CreateItemFinder(item.RewardItem.RowId),
                ImGuiContextMenu.CreateCopyItemName(item.RewardItem.RowId),
                ImGuiContextMenu.CreateItemSearch(item.RewardItem.RowId),
                ImGuiContextMenu.CreateOpenOnGarlandTools(item.RewardItem.RowId),
            }
            .Draw();

            ImGui.SameLine(32 * scale + ImGui.GetStyle().ItemSpacing.X);
            ImGuiUtils.PushCursorY(6 * scale);
            if (item.RewardItemCountLoss > 0)
            {
                ImGui.TextUnformatted(t("CurrencyReward.MinMax", item.RewardItemCountLoss, item.RewardItemCount));
            }
            else
            {
                ImGui.TextUnformatted(t("CurrencyReward.Normal", item.RewardItemCount));
            }
        }
    }
}
