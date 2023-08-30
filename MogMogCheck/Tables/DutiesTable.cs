using System.Collections.Generic;
using Dalamud.Interface;
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
        public override string ToName(Duty row)
        {
            var name = row.ContentFinderCondition!.Name.RawString;
            return char.ToUpper(name[0]) + name[1..];
        }

        public override unsafe void DrawColumn(Duty row, int _)
        {
            var scale = ImGuiHelpers.GlobalScale;

            var clicked = ImGui.Selectable($"##Duty{row.ContentFinderCondition!.RowId}", false, ImGuiSelectableFlags.None, new(ImGui.GetContentRegionAvail().X, 32 * scale));
            /*
            if (ImGui.IsItemHovered() && !ImGui.IsKeyDown(ImGuiKey.LeftAlt))
            {
                using var tooltip = ImRaii.Tooltip();
                Service.TextureManager.GetIcon(row.ContentFinderCondition.Image).Draw(new Vector2(1123, 360) * 0.5f);
            }
            */

            ImGui.SameLine(1, ImGui.GetStyle().ItemInnerSpacing.X);
            ImGuiUtils.PushCursorY(6 * scale);
            ImGui.TextUnformatted(ToName(row));
            if (clicked)
            {
                AgentContentsFinder.Instance()->OpenRegularDuty(row.ContentFinderCondition.RowId);
            }
        }
    }

    private sealed class RewardColumn : Column<Duty>
    {
        public override float Width
            => 130;

        public override int Compare(Duty lhs, Duty rhs)
            => lhs.RewardItemCount.CompareTo(rhs.RewardItemCount);

        public override void DrawColumn(Duty row, int _)
        {
            var iconSize = 28 * ImGuiHelpers.GlobalScale;
            var rowHeight = 32 * ImGuiHelpers.GlobalScale;
            var iconPadding = (rowHeight - iconSize) * 0.5f;

            ImGuiUtils.PushCursorY(iconPadding);
            Service.TextureManager.GetIcon(row.RewardItem!.Icon).Draw(iconSize);

            new ImGuiContextMenu($"##{row.ContentFinderCondition!.RowId}_ItemContextMenu{row.RewardItem.RowId}_Tooltip")
            {
                ImGuiContextMenu.CreateItemFinder(row.RewardItem.RowId),
                ImGuiContextMenu.CreateCopyItemName(row.RewardItem.RowId),
                ImGuiContextMenu.CreateItemSearch(row.RewardItem.RowId),
                ImGuiContextMenu.CreateOpenOnGarlandTools(row.RewardItem.RowId),
            }
            .Draw();

            ImGui.SameLine(iconSize + ImGui.GetStyle().ItemSpacing.X);
            ImGuiUtils.PushCursorY((rowHeight - ImGui.GetTextLineHeight() - iconPadding * 2f) * 0.5f);
            if (row.RewardItemCountLoss > 0)
            {
                ImGui.TextUnformatted(t("CurrencyReward.PvP", row.RewardItemCount, row.RewardItemCountLoss, row.RewardItemCountTie));
                if (ImGui.IsItemHovered())
                    ImGui.SetTooltip(t("CurrencyReward.PvP.Tooltip", row.RewardItemCount, row.RewardItemCountLoss, row.RewardItemCountTie));
            }
            else
            {
                ImGui.TextUnformatted(t("CurrencyReward.Normal", row.RewardItemCount));
            }
        }
    }
}
