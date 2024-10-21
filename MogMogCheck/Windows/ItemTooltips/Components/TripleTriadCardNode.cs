using Dalamud.Interface.Utility;
using HaselCommon.Gui.Yoga;
using HaselCommon.Gui.Yoga.Components;
using HaselCommon.Services;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using MogMogCheck.Services;
using YogaSharp;

namespace MogMogCheck.Windows.ItemTooltips.Components;

public class TripleTriadCardNode : Node
{
    private readonly TextureService _textureService;
    private readonly ExcelService _excelService;
    private readonly TripleTriadNumberFontManager _tripleTriadNumberFontManager;
    private readonly TripleTriadCardStars _cardStars;
    private readonly UldImage _cardType;
    private readonly TripleTriadCardNumbers _cardNumbers;
    private Item? _item;
    private uint _cardRowId;

    public TripleTriadCardNode(
        TextureService textureService,
        ExcelService excelService,
        TripleTriadNumberFontManager tripleTriadNumberFontManager) : base()
    {
        _textureService = textureService;
        _excelService = excelService;
        _tripleTriadNumberFontManager = tripleTriadNumberFontManager;

        Width = 208;
        Height = 256;
        JustifyContent = YGJustify.SpaceBetween;

        Add(new Node()
        {
            Margin = YGValue.Percent(4),
            FlexDirection = YGFlexDirection.Row,
            JustifyContent = YGJustify.SpaceBetween,
            Children = [
                _cardStars = new TripleTriadCardStars(textureService)
                {
                    Margin = YGValue.Percent(2),
                },
                _cardType = new UldImage()
                {
                    UldName = "CardTripleTriad",
                    PartListId = 1,
                    PartIndex = 1,
                    Scale = 0.75f,
                    Display = YGDisplay.None
                }
            ]
        });

        Add(_cardNumbers = new TripleTriadCardNumbers(tripleTriadNumberFontManager)
        {
            Display = YGDisplay.None,
            AlignSelf = YGAlign.Center,
            MarginBottom = YGValue.Percent(4)
        });
    }

    public void SetItem(Item item)
    {
        if (_item != item)
        {
            _item = item;

            var cardSizeScaled = ImGuiHelpers.ScaledVector2(208, 256);
            Width = cardSizeScaled.X;
            Height = cardSizeScaled.Y;

            _cardRowId = _item.ItemAction.Value!.Data[0];
            var cardResident = _excelService.GetRow<TripleTriadCardResident>(_cardRowId)!;
            var cardRarity = cardResident.TripleTriadCardRarity.Value!;

            _cardStars.Stars = cardRarity.Stars;

            _cardType.Display = cardResident.TripleTriadCardType.Row != 0 ? YGDisplay.Flex : YGDisplay.None;
            _cardType.PartIndex = cardResident.TripleTriadCardType.Row + 2;

            _cardNumbers.SetCard(cardResident);
            _cardNumbers.Display = YGDisplay.Flex;
        }
    }

    public override void DrawContent()
    {
        if (_item == null)
            return;

        var cardSize = ComputedSize;

        // background
        var pos = ImGui.GetCursorPos();
        _textureService.DrawPart("CardTripleTriad", 1, 0, cardSize);

        // card image
        ImGui.SetCursorPos(pos);
        _textureService.DrawIcon(87000 + _cardRowId, cardSize);
    }
}
