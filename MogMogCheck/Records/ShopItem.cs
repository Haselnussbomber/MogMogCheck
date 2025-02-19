namespace MogMogCheck.Records;

public record struct ShopItem(int Index, ItemEntry[] ReceiveItems, ItemEntry[] GiveItems);
