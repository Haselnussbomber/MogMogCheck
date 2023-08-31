using HaselCommon.Sheets;
using Lumina.Excel.GeneratedSheets;
using static MogMogCheck.Sheets.ExtendedSpecialShop;

namespace MogMogCheck.Records;

public record Reward
{
    public Reward(int Index, SpecialShopItem row)
    {
        this.Index = Index;

        ReceiveItems = new (ExtendedItem?, uint)[] {
            (row.ReceiveItemId1 != 0 ? GetRow<ExtendedItem>((uint)row.ReceiveItemId1) : null, row.ReceiveCount1),
            (row.ReceiveItemId2 != 0 ? GetRow<ExtendedItem>((uint)row.ReceiveItemId2) : null, row.ReceiveCount2)
        };

        GiveItems = new (ExtendedItem?, uint)[] {
            (row.GiveItemId1 != 0 ? GetRow<ExtendedItem>((uint)row.GiveItemId1) : null, row.GiveCount1),
            (row.GiveItemId2 != 0 ? GetRow<ExtendedItem>((uint)row.GiveItemId2) : null, row.GiveCount2),
            (row.GiveItemId2 != 0 ? GetRow<ExtendedItem>((uint)row.GiveItemId2) : null, row.GiveCount2)
        };

        RequiredQuest = row.UnlockQuest != 0 ? GetRow<Quest>((uint)row.UnlockQuest) : null;
    }

    public int Index { get; }
    public (ExtendedItem? Item, uint Quantity)[] ReceiveItems { get; }
    public (ExtendedItem? Item, uint Quantity)[] GiveItems { get; }
    public Quest? RequiredQuest { get; }
}
