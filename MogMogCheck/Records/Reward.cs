using HaselCommon.Sheets;

namespace MogMogCheck.Records;

public record Reward(uint Index, Item Item, uint Quantity, Item RequiredItem, uint RequiredCount);
