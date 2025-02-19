using HaselCommon.Services;
using HaselCommon.Yoga;
using Lumina.Excel.Sheets;
using MogMogCheck.Windows.ItemTooltips.Components;
using YogaSharp;

namespace MogMogCheck.Windows.ItemTooltips;

[RegisterTransient, AutoConstruct]
public partial class TripleTriadCardTooltip : Node
{
    private readonly ExcelService _excelService;
    private readonly SeStringEvaluatorService _seStringEvaluator;
    private readonly TripleTriadCardNode _card;
    private TextNode _infoLine;
    private uint _cardRowId;

    [AutoPostConstruct]
    private void Initialize()
    {
        Margin = 8;
        AlignItems = YGAlign.Center;
        RowGap = 4;

        Add(
            _infoLine = new TextNode(),
            _card
        );
    }

    public void SetItem(Item item)
    {
        SetCard(item.ItemAction.Value!.Data[0]);
    }

    public void SetCard(uint cardId)
    {
        if (_cardRowId == cardId)
            return;

        if (!_excelService.TryGetRow<TripleTriadCard>(cardId, out var cardRow))
            return;

        if (!_excelService.TryGetRow<TripleTriadCardResident>(cardId, out var cardResident))
            return;

        _cardRowId = cardId;

        var isEx = cardResident.UIPriority == 5;
        var order = (uint)cardResident.Order;
        var addonRowId = isEx ? 9773u : 9772;

        _infoLine.Text = $"{_seStringEvaluator.EvaluateFromAddon(addonRowId, [order]).ExtractText()} - {cardRow.Name}";
        _card.SetCard(_cardRowId, cardResident);
    }
}
