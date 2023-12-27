using System.Collections.Generic;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Utility.Table;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using HaselCommon.Enums;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Data.Files;
using Lumina.Excel.GeneratedSheets;
using MogMogCheck.Records;
using Companion = Lumina.Excel.GeneratedSheets.Companion;
using Ornament = Lumina.Excel.GeneratedSheets.Ornament;

namespace MogMogCheck.Tables;

public class RewardsTable : Table<Reward>
{
    private static readonly Dictionary<uint, Vector2?> IconSizeCache = new();

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
            => 80 * ImGuiHelpers.GlobalScale;

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

            var itemRow = row.ReceiveItems[0].Item!;
            var itemId = itemRow.RowId;
            var stackSize = itemRow.StackSize;

            if (!Plugin.Config.TrackedItems.TryGetValue(itemId, out var savedAmount))
                savedAmount = 0;

            var inputAmount = (int)savedAmount;

            ImGuiUtils.PushCursor(ImGui.GetStyle().ItemInnerSpacing.X * scale, paddingY);
            ImGui.SetNextItemWidth(-1);

            var changed = ImGui.DragInt($"##Row{idx}", ref inputAmount, 1, 0, (int)stackSize, $"%d / {stackSize}");

            if (inputAmount > stackSize)
                inputAmount = (int)stackSize;

            if (ImGui.IsItemHovered() || ImGui.IsItemActive())
            {
                if (inputAmount <= 1)
                    ImGui.SetTooltip(t("Reward.AmountInput.Tooltip.ResultOnly", inputAmount * row.GiveItems[0].Quantity));
                else
                    ImGui.SetTooltip(t("Reward.AmountInput.Tooltip.Calculation", inputAmount, row.GiveItems[0].Quantity, inputAmount * row.GiveItems[0].Quantity));
            }

            if (changed && savedAmount != inputAmount)
            {
                if (inputAmount > 0)
                {
                    if (!Plugin.Config.TrackedItems.ContainsKey(itemId))
                        Plugin.Config.TrackedItems.Add(itemId, (uint)inputAmount);
                    else
                        Plugin.Config.TrackedItems[itemId] = (uint)inputAmount;
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


            if (ImGui.IsItemHovered() && !ImGui.IsKeyDown(ImGuiKey.LeftAlt))
            {
                if (item.ItemAction.Value?.Type == (uint)ItemActionType.Mounts)
                {
                    using var tooltip = ImRaii.Tooltip();
                    var mount = GetRow<Mount>(item.ItemAction.Value!.Data[0])!;
                    Service.TextureManager.GetIcon(64000 + mount.Icon).Draw(192);
                }
                else if (item.ItemAction.Value?.Type == (uint)ItemActionType.Minions)
                {
                    using var tooltip = ImRaii.Tooltip();
                    var companion = GetRow<Companion>(item.ItemAction.Value!.Data[0])!;
                    Service.TextureManager.GetIcon(64000 + companion.Icon).Draw(192);
                }
                else if (item.ItemAction.Value?.Type == (uint)ItemActionType.FashionAccessories)
                {
                    using var tooltip = ImRaii.Tooltip();
                    var ornament = GetRow<Ornament>(item.ItemAction.Value!.Data[0])!;
                    Service.TextureManager.GetIcon(59000 + ornament.Icon).Draw(192);
                }
                else if (item.ItemAction.Value?.Type == (uint)ItemActionType.Miscellaneous && item.ItemAction.Value?.Data[1] == 5211) // Emotes
                {
                    using var tooltip = ImRaii.Tooltip();
                    var emote = GetRow<Emote>(item.ItemAction.Value!.Data[2])!;
                    Service.TextureManager.GetIcon(emote.Icon).Draw(80);
                }
                else if (item.ItemAction.Value?.Type == (uint)ItemActionType.Cards)
                {
                    using var tooltip = ImRaii.Tooltip();
                    var pos = ImGui.GetCursorPos();
                    Service.TextureManager.GetPart("CardTripleTriad", 1, 0).Draw(208, 256);
                    ImGui.SetCursorPos(pos);
                    Service.TextureManager.GetIcon(82100 + item.ItemAction.Value!.Data[0]).Draw(208, 256);
                }
                else if (item.ItemUICategory.Row == 95) // Paintings
                {
                    var pictureId = (uint)GetRow<Picture>(item.AdditionalData)!.Image;

                    if (!IconSizeCache.TryGetValue(pictureId, out var size))
                    {
                        var iconPath = Service.TextureProvider.GetIconPath(pictureId);
                        if (string.IsNullOrEmpty(iconPath))
                        {
                            IconSizeCache.Add(pictureId, null);
                        }
                        else
                        {
                            var file = Service.DataManager.GetFile<TexFile>(iconPath);
                            IconSizeCache.Add(pictureId, size = file != null ? new(file.Header.Width, file.Header.Height) : null);
                        }
                    }

                    if (size != null)
                    {
                        using var tooltip = ImRaii.Tooltip();
                        Service.TextureManager.GetIcon(pictureId).Draw(size * 0.5f);
                    }
                }
                else if (item.ItemAction.Value?.Type == (uint)ItemActionType.Miscellaneous && FindRow<CharaMakeCustomize>(row => row?.HintItem.Row == item.RowId) != null) // Hairstyles etc.
                {
                    using var tooltip = ImRaii.Tooltip();

                    var tribeId = 1;
                    var isMale = false;
                    unsafe
                    {
                        var character = (Character*)(Service.ClientState.LocalPlayer?.Address ?? 0);
                        if (character != null)
                        {
                            tribeId = character->DrawData.CustomizeData.Clan;
                            isMale = character->DrawData.CustomizeData.Sex == 0;
                        }
                    }

                    //! https://github.com/ufx/GarlandTools/blob/a241dd8/Garland.Data/Modules/NPCs.cs#L434-L464
                    var startIndex = tribeId switch
                    {
                        1 => isMale ? 0 : 100, // Midlander
                        2 => isMale ? 200 : 300, // Highlander
                        3 or 4 => isMale ? 400 : 500, // Wildwood / Duskwight
                        5 or 6 => isMale ? 600 : 700, // Plainsfolks / Dunesfolk
                        7 or 8 => isMale ? 800 : 900, // Seeker of the Sun / Keeper of the Moon
                        9 or 10 => isMale ? 1000 : 1100, // Sea Wolf / Hellsguard
                        11 or 12 => isMale ? 1200 : 1300, // Raen / Xaela
                        13 or 14 => 1400, // Helions / The Lost
                        15 or 16 => isMale ? 1600 : 1700, // Rava / Veena
                        _ => 0
                    };

                    var charaMakeCustomize = FindRow<CharaMakeCustomize>(row => row?.RowId >= startIndex && row.HintItem.Row == item.RowId);
                    if (charaMakeCustomize != null)
                    {
                        Service.TextureManager.GetIcon(charaMakeCustomize.Icon).Draw(192);
                    }
                }
                else
                {
                    using var tooltip = ImRaii.Tooltip();
                    Service.TextureManager.GetIcon(item.Icon).Draw(64);
                }
            }

            new ImGuiContextMenu($"##{idx}_ItemContextMenu{item.RowId}_IconTooltip")
            {
                ImGuiContextMenu.CreateItemFinder(item.RowId),
                ImGuiContextMenu.CreateCopyItemName(item.RowId),
                ImGuiContextMenu.CreateItemSearch(item),
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
                ImGuiContextMenu.CreateItemSearch(item),
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
                ImGui.TextUnformatted(item.ItemUICategory.Value!.Name);

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
                ImGuiContextMenu.CreateItemSearch(item),
                ImGuiContextMenu.CreateOpenOnGarlandTools(item.RowId),
            }
            .Draw();

            ImGui.SameLine(iconSize + itemSpacing.X);
            ImGuiUtils.PushCursorY(paddingY);
            ImGui.TextUnformatted(t("CurrencyReward.Normal", quantity));
        }
    }
}
