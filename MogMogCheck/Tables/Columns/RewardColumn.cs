using System.Numerics;
using Dalamud.Interface.Textures;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using HaselCommon.Game.Enums;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Gui.ImGuiTable;
using HaselCommon.Services;
using HaselCommon.Utils;
using Lumina.Excel.Sheets;
using MogMogCheck.Config;
using MogMogCheck.Extensions;
using MogMogCheck.Records;
using MogMogCheck.Services;

namespace MogMogCheck.Tables;

[RegisterSingleton, AutoConstruct]
public partial class RewardColumn : ColumnString<ShopItem>
{
    private readonly ExcelService _excelService;
    private readonly UldService _uldService;
    private readonly ItemService _itemService;
    private readonly ImGuiContextMenuService _imGuiContextMenuService;
    private readonly ITextureProvider _textureProvider;
    private readonly ISeStringEvaluator _seStringEvaluator;
    private readonly TripleTriadNumberFont _tripleTriadNumberFont;
    private readonly PluginConfig _pluginConfig;

    public override string ToName(ShopItem row)
        => row.ReceiveItems[0].Item.Name.ToString();

    public override void DrawColumn(ShopItem row)
    {
        ImGuiUtils.PushCursorY(MathF.Round(ImGui.GetStyle().FramePadding.Y / 2f)); // my cell padding

        var (item, quantity) = row.ReceiveItems[0];
        var iconSize = ImGui.GetFrameHeight();
        var isCollected = item.IsUnlockedOrCollected();
        var grayOut = _pluginConfig.GrayOutCollectedItems && isCollected;

        ImGui.BeginGroup();

        _textureProvider.DrawIcon(new GameIconLookup(item.Icon, item.IsHighQuality), new DrawInfo(iconSize)
        {
            TintColor = grayOut ? (Vector4)Color.Grey : null
        });

        ImGui.SameLine();

        using (Color.From(ImGuiCol.TextDisabled).Push(ImGuiCol.Text, grayOut))
            ImGui.Selectable($"{(quantity > 1 ? $"{quantity}x " : string.Empty)}{item.Name}##Selectable{row.Index}", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, iconSize - ImGui.GetStyle().FramePadding.Y));

        ImGui.EndGroup();

        if (ImGui.IsItemHovered() && !ImGui.IsKeyDown(ImGuiKey.LeftAlt))
        {
            DrawItemTooltip(item);
        }

        _imGuiContextMenuService.Draw($"##RewardColumnContextMenu{item}", builder =>
        {
            builder.AddTryOn(item);
            builder.AddItemFinder(item);
            builder.AddLinkItem(item);
            builder.AddCopyItemName(item);
            builder.AddItemSearch(item);
            builder.AddOpenOnGarlandTools("item", item);
        });

        if (isCollected)
        {
            ImGui.SameLine(1, 0);

            if (_textureProvider.GetFromGame("ui/uld/RecipeNoteBook_hr1.tex").TryGetWrap(out var tex, out _))
            {
                var pos = ImGui.GetCursorScreenPos() + new Vector2((float)iconSize / 2.1f);
                ImGui.GetWindowDrawList().AddImage(tex.Handle, pos, pos + new Vector2((float)iconSize / 1.5f), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
            }
        }
    }

    public unsafe void DrawItemTooltip(ItemHandle item, string? descriptionOverride = null)
    {
        if (!item.TryGetItem(out var itemRow) || !_textureProvider.TryGetFromGameIcon(item.Icon, out var tex) || !tex.TryGetWrap(out var icon, out _))
            return;

        using var id = ImRaii.PushId("##ItemTooltip");

        using var tooltip = ImRaii.Tooltip();
        if (!tooltip) return;

        using var popuptable = ImRaii.Table("PopupTable", 2, ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.NoSavedSettings | ImGuiTableFlags.NoKeepColumnsVisible);
        if (!popuptable) return;

        var itemInnerSpacing = ImGui.GetStyle().ItemInnerSpacing * ImGuiHelpers.GlobalScale;
        var title = item.Name.ToString();

        ImGui.TableSetupColumn("Icon", ImGuiTableColumnFlags.WidthFixed, 40 * ImGuiHelpers.GlobalScale + itemInnerSpacing.X);
        ImGui.TableSetupColumn("Text", ImGuiTableColumnFlags.WidthFixed, Math.Max(ImGui.CalcTextSize(title).X + itemInnerSpacing.X, 300 * ImGuiHelpers.GlobalScale));

        ImGui.TableNextColumn(); // Icon
        ImGui.Image(icon.Handle, ImGuiHelpers.ScaledVector2(40));

        var isUnlocked = ((ItemHandle)item).IsUnlockedOrCollected();
        if (isUnlocked)
        {
            ImGui.SameLine(1 + ImGui.GetStyle().CellPadding.X + itemInnerSpacing.X, 0);

            if (_textureProvider.GetFromGame("ui/uld/RecipeNoteBook_hr1.tex").TryGetWrap(out var checkTex, out _))
            {
                var pos = ImGui.GetCursorScreenPos() + ImGuiHelpers.ScaledVector2(40) / 2.1f;
                ImGui.GetWindowDrawList().AddImage(checkTex.Handle, pos, pos + new Vector2(40 * ImGuiHelpers.GlobalScale / 1.5f), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
            }
        }

        ImGui.TableNextColumn(); // Text
        using var indentSpacing = ImRaii.PushStyle(ImGuiStyleVar.IndentSpacing, itemInnerSpacing.X);
        using var indent = ImRaii.PushIndent(1);

        ImGui.Text(title);

        if (isUnlocked)
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() - 40 * ImGuiHelpers.GlobalScale / 2f - 3); // wtf

        var category = itemRow.ItemUICategory.IsValid ? itemRow.ItemUICategory.Value.Name.ToString() : null;
        if (!string.IsNullOrEmpty(category))
        {
            ImGuiUtils.PushCursorY(-3 * ImGuiHelpers.GlobalScale);
            using (ImRaii.PushColor(ImGuiCol.Text, Color.Grey))
                ImGui.Text(category);
        }

        var description = descriptionOverride ?? (!item.Description.IsEmpty ? item.Description.ToString() : null);
        if (!string.IsNullOrEmpty(description))
        {
            DrawSeparator(marginTop: 1, marginBottom: 4);

            ImGui.TextWrapped(description);
        }

        switch (item.ItemActionType)
        {
            case ItemActionType.Mount when _excelService.TryGetRow<Mount>(itemRow.ItemAction.Value.Data[0], out var mount):
                _textureProvider.DrawIcon(64000 + mount.Icon, new DrawInfo() { Scale = 0.5f * ImGuiHelpers.GlobalScale });
                break;

            case ItemActionType.Companion when _excelService.TryGetRow<Companion>(itemRow.ItemAction.Value.Data[0], out var companion):
                _textureProvider.DrawIcon(64000 + companion.Icon, new DrawInfo() { Scale = 0.5f * ImGuiHelpers.GlobalScale });
                break;

            case ItemActionType.Ornament when _excelService.TryGetRow<Ornament>(itemRow.ItemAction.Value.Data[0], out var ornament):
                _textureProvider.DrawIcon(59000 + ornament.Icon, new DrawInfo() { Scale = 0.5f * ImGuiHelpers.GlobalScale });
                break;

            case ItemActionType.UnlockLink when itemRow.ItemAction.Value.Data[1] == 5211 && _excelService.TryGetRow<Emote>(itemRow.ItemAction.Value.Data[2], out var emote):
                _textureProvider.DrawIcon((uint)emote.Icon, new DrawInfo() { Scale = 0.5f * ImGuiHelpers.GlobalScale });
                break;

            case ItemActionType.UnlockLink when itemRow.ItemAction.Value.Data[1] == 4659 && _itemService.GetHairstyleIconId(item) is { } hairStyleIconId && hairStyleIconId != 0:
                _textureProvider.DrawIcon(hairStyleIconId, new DrawInfo() { Scale = ImGuiHelpers.GlobalScale });
                break;

            case ItemActionType.UnlockLink when itemRow.ItemAction.Value.Data[1] == 9390 && _itemService.GetFacePaintIconId(item) is { } facePaintIconId && facePaintIconId != 0:
                _textureProvider.DrawIcon(facePaintIconId, new DrawInfo() { Scale = ImGuiHelpers.GlobalScale });
                break;

            case ItemActionType.TripleTriadCard:
                if (_excelService.TryGetRow<TripleTriadCardResident>(itemRow.ItemAction.Value.Data[0], out var residentRow) &&
                    _excelService.TryGetRow<TripleTriadCardObtain>(residentRow.AcquisitionType.RowId, out var obtainRow) &&
                    obtainRow.Text.RowId != 0)
                {
                    DrawSeparator();
                    _textureProvider.DrawIcon(obtainRow.Icon, 40 * ImGuiHelpers.GlobalScale);
                    ImGui.SameLine();
                    ImGui.TextWrapped(_seStringEvaluator.EvaluateFromAddon(obtainRow.Text.RowId, [
                        residentRow.Acquisition.RowId,
                        residentRow.Location.RowId
                    ]).ToString());
                }

                DrawTripleTriadCard(item);
                break;

            default:
                if (itemRow.ItemUICategory.RowId == 95 // Paintings
                    && _excelService.TryGetRow<Picture>(itemRow.AdditionalData.RowId, out var picture)
                    && _textureProvider.TryGetIconSize((uint)picture.Image, out var pictureSize))
                {
                    var maxSize = new Vector2(ImGui.GetContentRegionAvail().X, 1);
                    _textureProvider.DrawIcon(picture.Image, pictureSize.Cover(maxSize));
                }
                break;
        }
    }

    private void DrawTripleTriadCard(uint cardId)
    {
        if (!_excelService.TryGetRow<TripleTriadCard>(cardId, out var card))
            return;

        if (!_excelService.TryGetRow<TripleTriadCardResident>(cardId, out var cardResident))
            return;

        DrawSeparator(marginTop: 3);

        var isEx = cardResident.UIPriority == 5;
        var order = (uint)cardResident.Order;
        var addonRowId = isEx ? 9773u : 9772;

        var infoText = $"{_seStringEvaluator.EvaluateFromAddon(addonRowId, [order]).ExtractText()} - {card.Name}";
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() - ImGui.GetStyle().IndentSpacing + ImGui.GetContentRegionAvail().X / 2f - ImGui.CalcTextSize(infoText).X / 2f);
        ImGui.Text(infoText);

        var cardSizeScaled = ImGuiHelpers.ScaledVector2(208, 256);
        var cardStartPosX = ImGui.GetCursorPosX() - ImGui.GetStyle().IndentSpacing + ImGui.GetContentRegionAvail().X / 2f - cardSizeScaled.X / 2f;
        var cardStartPos = new Vector2(cardStartPosX, ImGui.GetCursorPosY());

        // draw background
        ImGui.SetCursorPosX(cardStartPosX);
        _uldService.DrawPart("CardTripleTriad", 1, 0, cardSizeScaled);

        // draw card
        ImGui.SetCursorPos(cardStartPos);
        _textureProvider.DrawIcon(87000 + cardId, cardSizeScaled);

        // draw numbers
        using var font = _tripleTriadNumberFont.Push();

        var letterSize = ImGui.CalcTextSize("A");
        var scaledLetterSize = letterSize / 2f;
        var pos = cardStartPos + new Vector2(cardSizeScaled.X / 2f, cardSizeScaled.Y - letterSize.Y * 1.5f) - letterSize;

        var positionTop = pos + new Vector2(scaledLetterSize.X, -scaledLetterSize.Y);
        var positionBottom = pos + new Vector2(scaledLetterSize.X, scaledLetterSize.Y);
        var positionRight = pos + new Vector2(letterSize.X * 1.1f + scaledLetterSize.X, 0);
        var positionLeft = pos + new Vector2(-(letterSize.X * 0.1f + scaledLetterSize.X), 0);

        var textTop = $"{cardResident.Top:X}";
        var textBottom = $"{cardResident.Bottom:X}";
        var textRight = $"{cardResident.Right:X}";
        var textLeft = $"{cardResident.Left:X}";

        DrawTextShadow(positionTop, textTop);
        DrawTextShadow(positionBottom, textBottom);
        DrawTextShadow(positionRight, textRight);
        DrawTextShadow(positionLeft, textLeft);

        DrawText(positionTop, textTop);
        DrawText(positionBottom, textBottom);
        DrawText(positionRight, textRight);
        DrawText(positionLeft, textLeft);

        // draw stars
        var cardRarity = cardResident.TripleTriadCardRarity.Value!;

        var starSize = 32 * 0.75f * ImGuiHelpers.GlobalScale;
        var starRadius = starSize / 1.666f;
        var starCenter = cardStartPos + ImGuiHelpers.ScaledVector2(14) + new Vector2(starSize) / 2f;

        if (cardRarity.Stars >= 1)
        {
            DrawStar(StarPosition.Top);

            if (cardRarity.Stars >= 2)
                DrawStar(StarPosition.Left);
            if (cardRarity.Stars >= 3)
                DrawStar(StarPosition.Right);
            if (cardRarity.Stars >= 4)
                DrawStar(StarPosition.BottomLeft);
            if (cardRarity.Stars >= 5)
                DrawStar(StarPosition.BottomRight);
        }

        // draw type
        if (cardResident.TripleTriadCardType.RowId != 0)
        {
            var typeSize = 32 * ImGuiHelpers.GlobalScale;

            var partIndex = cardResident.TripleTriadCardType.RowId switch
            {
                4 => 2u,
                _ => cardResident.TripleTriadCardType.RowId + 2
            };

            ImGui.SetCursorPos(cardStartPos + new Vector2(cardSizeScaled.X - typeSize * 1.5f, typeSize / 2.5f));
            _uldService.DrawPart("CardTripleTriad", 1, partIndex, typeSize);
        }

        // functions

        void DrawStar(StarPosition pos)
        {
            var angleIncrement = 2 * MathF.PI / 5; // 5 = amount of stars
            var angle = (int)pos * angleIncrement - MathF.PI / 2;

            ImGui.SetCursorPos(starCenter + new Vector2(starRadius * MathF.Cos(angle), starRadius * MathF.Sin(angle)));
            _uldService.DrawPart("CardTripleTriad", 1, 1, starSize);
        }
    }

    private static void DrawTextShadow(Vector2 position, string text)
    {
        DrawShadow(position, ImGui.CalcTextSize(text), 8, Color.Black with { A = 0.1f });
    }

    private static void DrawText(Vector2 position, string text)
    {
        var outlineColor = Color.Black with { A = 0.5f };

        // outline
        ImGui.SetCursorPos(position + ImGuiHelpers.ScaledVector2(-1));
        using (outlineColor.Push(ImGuiCol.Text))
            ImGui.Text(text);

        ImGui.SetCursorPos(position + ImGuiHelpers.ScaledVector2(1));
        using (outlineColor.Push(ImGuiCol.Text))
            ImGui.Text(text);

        ImGui.SetCursorPos(position + ImGuiHelpers.ScaledVector2(1, -1));
        using (outlineColor.Push(ImGuiCol.Text))
            ImGui.Text(text);

        ImGui.SetCursorPos(position + ImGuiHelpers.ScaledVector2(-1, 1));
        using (outlineColor.Push(ImGuiCol.Text))
            ImGui.Text(text);

        // text
        ImGui.SetCursorPos(position);
        ImGui.Text(text);
    }

    private static void DrawShadow(Vector2 pos, Vector2 size, int layers, Vector4 shadowColor)
    {
        var drawList = ImGui.GetWindowDrawList();

        for (var i = 0; i < layers; i++)
        {
            var shadowOffset = i * 2.0f;
            var transparency = shadowColor.W * (1.0f - (float)i / layers);
            var currentShadowColor = new Vector4(shadowColor.X, shadowColor.Y, shadowColor.Z, transparency);

            drawList.AddRectFilled(
                pos - new Vector2(shadowOffset, shadowOffset),
                pos + size + new Vector2(shadowOffset, shadowOffset),
                ImGui.ColorConvertFloat4ToU32(currentShadowColor),
                50
            );
        }
    }

    private enum StarPosition
    {
        Top = 0,
        Right = 1,
        Left = 4,
        BottomLeft = 3,
        BottomRight = 2
    }

    private static void DrawSeparator(float marginTop = 2, float marginBottom = 5)
    {
        ImGuiUtils.PushCursorY(marginTop * ImGuiHelpers.GlobalScale);
        var pos = ImGui.GetCursorScreenPos();
        ImGui.GetWindowDrawList().AddLine(pos, pos + new Vector2(ImGui.GetContentRegionAvail().X, 0), ImGui.GetColorU32(ImGuiCol.Separator));
        ImGuiUtils.PushCursorY(marginBottom * ImGuiHelpers.GlobalScale);
    }
}
