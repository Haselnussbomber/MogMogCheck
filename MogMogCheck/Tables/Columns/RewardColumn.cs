using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using HaselCommon.Extensions.Strings;
using HaselCommon.Game.Enums;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Gui.ImGuiTable;
using HaselCommon.Services;
using HaselCommon.Sheets;
using HaselCommon.Utils;
using ImGuiNET;
using Lumina.Data.Files;
using Lumina.Excel.Sheets;
using MogMogCheck.Records;
using MogMogCheck.Windows.ItemTooltips;

namespace MogMogCheck.Tables;

[RegisterSingleton, AutoConstruct]
public partial class RewardColumn : ColumnString<ShopItem>
{
    private readonly ExcelService _excelService;
    private readonly TextService _textService;
    private readonly TextureService _textureService;
    private readonly ItemService _itemService;
    private readonly ImGuiContextMenuService _imGuiContextMenuService;
    private readonly IDataManager _dataManager;
    private readonly ITextureProvider _textureProvider;
    private readonly SeStringEvaluatorService _seStringEvaluator;
    private readonly TripleTriadCardTooltip _tripleTriadCardTooltip;

    private readonly Dictionary<uint, Vector2> _iconSizeCache = [];

    public override string ToName(ShopItem row)
        => _textService.GetItemName(row.ReceiveItems[0].ItemId);

    public override void DrawColumn(ShopItem row)
    {
        ImGuiUtils.PushCursorY(ImGui.GetStyle().FramePadding.Y / 2f); // my cell padding

        var (itemId, quantity) = row.ReceiveItems[0];
        var iconSize = ImGui.GetFrameHeight();
        var isHovered = false;

        // Icon
        _textureService.DrawIcon(_itemService.GetIconId(itemId) + (itemId.IsHighQuality() ? 1_000_000u : 0), iconSize);
        isHovered |= ImGui.IsItemHovered();

        // Name
        ImGui.SameLine();
        ImGui.Selectable($"##Selectable{row.Index}", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, iconSize - ImGui.GetStyle().FramePadding.Y));
        isHovered |= ImGui.IsItemHovered();
        ImGui.SameLine(iconSize + ImGui.GetStyle().ItemSpacing.X, 0);
        ImGui.TextUnformatted((quantity > 1 ? $"{quantity}x " : string.Empty) + _textService.GetItemName(itemId));

        if (isHovered && !ImGui.IsKeyDown(ImGuiKey.LeftAlt) && _excelService.TryGetRow<Item>(itemId, out var item))
        {
            DrawItemTooltip(item);
        }

        _imGuiContextMenuService.Draw($"##ShopItemReward{row.Index}_ItemContextMenu{itemId}_IconTooltip", builder =>
        {
            builder.AddTryOn(itemId);
            builder.AddItemFinder(itemId);
            builder.AddCopyItemName(itemId);
            builder.AddItemSearch(itemId);
            builder.AddOpenOnGarlandTools("item", itemId);
        });

        if (_itemService.IsUnlockable(itemId) && _itemService.IsUnlocked(itemId))
        {
            ImGui.SameLine(1, 0);

            if (_textureProvider.GetFromGame("ui/uld/RecipeNoteBook_hr1.tex").TryGetWrap(out var tex, out _))
            {
                var pos = ImGui.GetCursorScreenPos() + new Vector2((float)iconSize / 2.1f);
                ImGui.GetWindowDrawList().AddImage(tex.ImGuiHandle, pos, pos + new Vector2((float)iconSize / 1.5f), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
            }
        }
    }

    public unsafe void DrawItemTooltip(Item item, string? descriptionOverride = null)
    {
        if (!_textureProvider.TryGetFromGameIcon((uint)item.Icon, out var tex) || !tex.TryGetWrap(out var icon, out _))
            return;

        using var id = ImRaii.PushId($"ItemTooltip{item.RowId}");

        using var tooltip = ImRaii.Tooltip();
        if (!tooltip) return;

        using var popuptable = ImRaii.Table("PopupTable", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.NoKeepColumnsVisible);
        if (!popuptable) return;

        var itemInnerSpacing = ImGui.GetStyle().ItemInnerSpacing * ImGuiHelpers.GlobalScale;
        var title = _textService.GetItemName(item.RowId);

        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 40 * ImGuiHelpers.GlobalScale + itemInnerSpacing.X);
        ImGui.TableSetupColumn("Text", ImGuiTableColumnFlags.WidthFixed, Math.Max(ImGui.CalcTextSize(title).X + itemInnerSpacing.X, 300 * ImGuiHelpers.GlobalScale));

        ImGui.TableNextColumn(); // Icon
        ImGui.Image(icon.ImGuiHandle, ImGuiHelpers.ScaledVector2(40));

        var isUnlocked = _itemService.IsUnlockable(item) && _itemService.IsUnlocked(item);
        if (isUnlocked)
        {
            ImGui.SameLine(1 + ImGui.GetStyle().CellPadding.X + itemInnerSpacing.X, 0);

            if (_textureProvider.GetFromGame("ui/uld/RecipeNoteBook_hr1.tex").TryGetWrap(out var checkTex, out _))
            {
                var pos = ImGui.GetCursorScreenPos() + ImGuiHelpers.ScaledVector2(40) / 2.1f;
                ImGui.GetWindowDrawList().AddImage(checkTex.ImGuiHandle, pos, pos + new Vector2(40 * ImGuiHelpers.GlobalScale / 1.5f), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
            }
        }

        ImGui.TableNextColumn(); // Text
        using var indentSpacing = ImRaii.PushStyle(ImGuiStyleVar.IndentSpacing, itemInnerSpacing.X);
        using var indent = ImRaii.PushIndent(1);

        ImGui.TextUnformatted(title);

        if (isUnlocked)
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 40 * ImGuiHelpers.GlobalScale / 2f - 3); // wtf

        var category = item.ItemUICategory.IsValid ? item.ItemUICategory.Value.Name.ExtractText().StripSoftHypen() : null;
        if (!string.IsNullOrEmpty(category))
        {
            ImGuiUtils.PushCursorY(-3 * ImGuiHelpers.GlobalScale);
            using (ImRaii.PushColor(ImGuiCol.Text, (uint)Color.Grey))
                ImGui.TextUnformatted(category);
        }

        var description = descriptionOverride ?? (!item.Description.IsEmpty ? item.Description.ExtractText().StripSoftHypen() : null);
        if (!string.IsNullOrEmpty(description))
        {
            DrawSeparator(marginTop: 1, marginBottom: 4);

            ImGuiHelpers.SafeTextWrapped(description);
        }

        if (item.ItemAction.Value.Type == (uint)ItemActionType.Mount)
        {
            if (_excelService.TryGetRow<Mount>(item.ItemAction.Value!.Data[0], out var mount))
            {
                _textureService.DrawIcon(64000 + mount.Icon, new DrawInfo() { Scale = 0.5f * ImGuiHelpers.GlobalScale });
            }
        }
        else if (item.ItemAction.Value.Type == (uint)ItemActionType.Companion)
        {
            if (_excelService.TryGetRow<Companion>(item.ItemAction.Value!.Data[0], out var companion))
            {
                _textureService.DrawIcon(64000 + companion.Icon, new DrawInfo() { Scale = 0.5f * ImGuiHelpers.GlobalScale });
            }
        }
        else if (item.ItemAction.Value.Type == (uint)ItemActionType.Ornament)
        {
            if (_excelService.TryGetRow<Ornament>(item.ItemAction.Value!.Data[0], out var ornament))
            {
                _textureService.DrawIcon(59000 + ornament.Icon, new DrawInfo() { Scale = 0.5f * ImGuiHelpers.GlobalScale });
            }
        }
        else if (item.ItemAction.Value.Type == (uint)ItemActionType.UnlockLink && item.ItemAction.Value.Data[1] == 5211) // Emotes
        {
            if (_excelService.TryGetRow<Emote>(item.ItemAction.Value!.Data[2], out var emote))
            {
                _textureService.DrawIcon(emote.Icon, new DrawInfo() { Scale = 0.5f * ImGuiHelpers.GlobalScale });
            }
        }
        else if (item.ItemAction.Value.Type == (uint)ItemActionType.UnlockLink && item.ItemAction.Value.Data[1] == 4659 && _itemService.GetHairstyleIconId(item.RowId) is { } hairStyleIconId && hairStyleIconId != 0) // Hairstyles
        {
            _textureService.DrawIcon(hairStyleIconId, new DrawInfo() { Scale = ImGuiHelpers.GlobalScale });
        }
        else if (item.ItemAction.Value.Type == (uint)ItemActionType.UnlockLink && item.ItemAction.Value.Data[1] == 9390) // Face Paints
        {
            // TODO: move to ItemService
            var playerState = PlayerState.Instance();
            if (playerState->IsLoaded == 1 &&
                _excelService.TryFindRow<CustomHairMakeType>(t => t.Tribe.RowId == playerState->Tribe && t.Gender == playerState->Sex, out var hairMakeType) &&
                _excelService.TryFindRow<CharaMakeCustomize>(row => row.IsPurchasable && row.Data == item.ItemAction.Value.Data[0] && hairMakeType.CharaMakeStruct[7].SubMenuParam.Any(id => id == row.RowId), out var charaMakeCustomize))
            {
                _textureService.DrawIcon(charaMakeCustomize.Icon, new DrawInfo());
            }
        }
        else if (item.ItemAction.Value.Type == (uint)ItemActionType.TripleTriadCard)
        {
            if (_excelService.TryGetRow<TripleTriadCardResident>(item.ItemAction.Value.Data[0], out var residentRow) &&
                _excelService.TryGetRow<TripleTriadCardObtain>(residentRow.AcquisitionType, out var obtainRow) &&
                obtainRow.Unknown1 != 0)
            {
                DrawSeparator();
                _textureService.DrawIcon(obtainRow.Unknown0, 40 * ImGuiHelpers.GlobalScale);
                ImGui.SameLine();
                ImGuiHelpers.SafeTextWrapped(_seStringEvaluator.EvaluateFromAddon(obtainRow.Unknown1, [
                    residentRow.Acquisition.RowId,
                    residentRow.Location.RowId
                ]).ExtractText().StripSoftHypen());
            }

            DrawSeparator(marginTop: 3);

            _tripleTriadCardTooltip.MarginTop = ImGui.GetCursorPosY();
            _tripleTriadCardTooltip.MarginLeft = ImGui.GetContentRegionAvail().X / 2f - 208 * ImGuiHelpers.GlobalScale / 2f + ImGui.GetCursorPosX() - itemInnerSpacing.X;
            _tripleTriadCardTooltip?.SetItem(item);
            _tripleTriadCardTooltip?.CalculateLayout();
            _tripleTriadCardTooltip?.Update();
            _tripleTriadCardTooltip?.Draw();
        }
        else if (item.ItemUICategory.RowId == 95 && _excelService.TryGetRow<Picture>(item.AdditionalData.RowId, out var picture)) // Paintings
        {
            _textureService.DrawIcon(picture.Image, ResizeToFit(GetIconSize((uint)picture.Image), ImGui.GetContentRegionAvail().X));
        }
    }

    // TODO: move to TextureService?
    private Vector2 GetIconSize(uint iconId)
    {
        if (_iconSizeCache.TryGetValue(iconId, out var size))
            return size;

        var iconPath = _textureProvider.GetIconPath(iconId);
        if (string.IsNullOrEmpty(iconPath))
        {
            _iconSizeCache.Add(iconId, size = Vector2.Zero);
            return size;
        }

        var file = _dataManager.GetFile<TexFile>(iconPath);
        _iconSizeCache.Add(iconId, size = file != null ? new Vector2(file.Header.Width, file.Header.Height) : Vector2.Zero);
        return size;
    }

    private static Vector2 ResizeToFit(Vector2 imageSize, float outerWidth)
    {
        if (imageSize.X <= outerWidth)
            return new Vector2(imageSize.X, imageSize.Y);

        var aspectRatio = imageSize.Y / imageSize.X;
        return new Vector2(outerWidth, outerWidth * aspectRatio);
    }

    private static void DrawSeparator(float marginTop = 2, float marginBottom = 5)
    {
        ImGuiUtils.PushCursorY(marginTop * ImGuiHelpers.GlobalScale);
        var pos = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddLine(pos, pos + new Vector2(ImGui.GetContentRegionAvail().X, 0), ImGui.GetColorU32(ImGuiCol.Separator));
        ImGuiUtils.PushCursorY(marginBottom * ImGuiHelpers.GlobalScale);
    }
}
