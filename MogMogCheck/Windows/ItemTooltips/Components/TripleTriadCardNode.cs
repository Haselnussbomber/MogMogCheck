using Dalamud.Interface.Utility;
using HaselCommon.Services;
using HaselCommon.Yoga;
using HaselCommon.Yoga.Components;
using ImGuiNET;
using Lumina.Excel.Sheets;
using YogaSharp;

namespace MogMogCheck.Windows.ItemTooltips.Components;

[RegisterTransient, AutoConstruct]
public partial class TripleTriadCardNode : Node
{
    private readonly TextureService _textureService;
    private readonly TripleTriadCardStars _cardStars;
    private readonly TripleTriadCardNumbers _cardNumbers;
    private UldImage _cardType;
    private uint _cardRowId;

    [AutoPostConstruct]
    private void Initialize()
    {
        JustifyContent = YGJustify.SpaceBetween;
        PositionType = YGPositionType.Relative;

        Add(new Node()
        {
            PositionType = YGPositionType.Absolute,
            Width = YGValue.Percent(100),
            Height = YGValue.Percent(25),
            Padding = YGValue.Percent(6),
            FlexDirection = YGFlexDirection.Row,
            JustifyContent = YGJustify.SpaceBetween,
            Children = [
                _cardStars,
                _cardType = new UldImage()
                {
                    AlignSelf = YGAlign.FlexStart,
                    UldName = "CardTripleTriad",
                    PartListId = 1,
                    PartIndex = 1,
                    Scale = 0.75f,
                    Display = YGDisplay.None,
                    Overflow = YGOverflow.Hidden
                }
            ]
        });

        Add(_cardNumbers);
    }

    public override void ApplyGlobalScale(float globalFontScale)
    {
        Width = 208 * globalFontScale;
        Height = 256 * globalFontScale;
    }

    internal void SetCard(uint cardRowId, TripleTriadCardResident cardResident)
    {
        var cardSizeScaled = ImGuiHelpers.ScaledVector2(208, 256);
        Width = cardSizeScaled.X;
        Height = cardSizeScaled.Y;

        _cardRowId = cardRowId;
        var cardRarity = cardResident.TripleTriadCardRarity.Value!;

        _cardStars.Stars = cardRarity.Stars;

        _cardType.Display = cardResident.TripleTriadCardType.RowId != 0 ? YGDisplay.Flex : YGDisplay.None;
        _cardType.PartIndex = cardResident.TripleTriadCardType.RowId switch
        {
            4 => 2,
            _ => cardResident.TripleTriadCardType.RowId + 2
        };

        _cardNumbers.SetCard(cardResident);
        _cardNumbers.Display = YGDisplay.Flex;
    }

    public override void DrawContent()
    {
        if (_cardRowId == 0)
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
