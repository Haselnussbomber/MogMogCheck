using HaselCommon.Services;
using Item = Lumina.Excel.GeneratedSheets.Item;
using Quest = Lumina.Excel.GeneratedSheets.Quest;
using SpecialShop = Lumina.Excel.GeneratedSheets2.SpecialShop;

namespace MogMogCheck.Records;

public record Reward
{
    public Reward(int Index, SpecialShop.ItemStruct item, ExcelService excelService)
    {
        this.Index = Index;

        ReceiveItems = [
            (excelService.GetRow<Item>(item.Item[0].Row), item.ReceiveCount[0]),
            (excelService.GetRow<Item>(item.Item[1].Row), item.ReceiveCount[1])
        ];

        GiveItems = [
            (excelService.GetRow<Item>((uint)item.ItemCost[0]), item.CurrencyCost[0]),
            (excelService.GetRow<Item>((uint)item.ItemCost[1]), item.CurrencyCost[1]),
            (excelService.GetRow<Item>((uint)item.ItemCost[2]), item.CurrencyCost[2])
        ];

        RequiredQuest = excelService.GetRow<Quest>(item.Quest.Row);
    }

    public int Index { get; }
    public (Item? Item, uint Quantity)[] ReceiveItems { get; }
    public (Item? Item, uint Quantity)[] GiveItems { get; }
    public Quest? RequiredQuest { get; }
}
