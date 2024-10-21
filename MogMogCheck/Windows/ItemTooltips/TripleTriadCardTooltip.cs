using HaselCommon.Gui.Yoga;
using HaselCommon.Services;
using Lumina.Excel.GeneratedSheets;
using MogMogCheck.Services;
using MogMogCheck.Windows.ItemTooltips.Components;
using YogaSharp;

namespace MogMogCheck.Windows.ItemTooltips;

public class TripleTriadCardTooltip : Node
{
    private readonly ExcelService _excelService;
    private readonly TextNode _infoLine;
    private readonly TripleTriadCardNode _card;
    private Item? _item;

    public TripleTriadCardTooltip(
        TextureService textureService,
        ExcelService excelService,
        TripleTriadNumberFontManager tripleTriadNumberFontManager) : base()
    {
        _excelService = excelService;

        Margin = 8;
        AlignItems = YGAlign.Center;
        RowGap = 4;

        Add(
            _infoLine = new TextNode(),
            _card = new TripleTriadCardNode(textureService, excelService, tripleTriadNumberFontManager)
        );
    }

    public void SetItem(Item item)
    {
        if (_item != item)
        {
            _item = item;

            var cardId = _item.ItemAction.Value!.Data[0];
            var cardRow = _excelService.GetRow<TripleTriadCard>(cardId)!;
            var cardResident = _excelService.GetRow<TripleTriadCardResident>(cardId)!;

            _infoLine.Text = $"{(cardResident.TripleTriadCardRarity.Row == 5 ? "Ex" : "No")}. {cardResident.Order} - {cardRow.Name}";
            _card.SetItem(item);
        }
    }
}
