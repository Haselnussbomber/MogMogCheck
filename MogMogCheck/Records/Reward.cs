using HaselCommon.Sheets;
using static MogMogCheck.Sheets.SpecialShop;
using Quest = Lumina.Excel.GeneratedSheets.Quest;

namespace MogMogCheck.Records;

public record Reward
{
    public Reward(int Index, SpecialShopItem row)
    {
        this.Index = Index;

        ReceiveItems = new (Item?, uint)[] {
            (row.ReceiveItemId1 != 0 ? GetRow<Item>((uint)row.ReceiveItemId1) : null, row.ReceiveCount1),
            (row.ReceiveItemId2 != 0 ? GetRow<Item>((uint)row.ReceiveItemId2) : null, row.ReceiveCount2)
        };

        GiveItems = new (Item?, uint)[] {
            (row.GiveItemId1 != 0 ? GetRow<Item>((uint)row.GiveItemId1) : null, row.GiveCount1),
            (row.GiveItemId2 != 0 ? GetRow<Item>((uint)row.GiveItemId2) : null, row.GiveCount2),
            (row.GiveItemId2 != 0 ? GetRow<Item>((uint)row.GiveItemId2) : null, row.GiveCount2)
        };

        RequiredQuest = row.UnlockQuest != 0 ? GetRow<Quest>((uint)row.UnlockQuest) : null;
    }

    public int Index { get; }
    public (Item? Item, uint Quantity)[] ReceiveItems { get; }
    public (Item? Item, uint Quantity)[] GiveItems { get; }
    public Quest? RequiredQuest { get; }
}
