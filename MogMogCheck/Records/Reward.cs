using HaselCommon.Sheets;

namespace MogMogCheck.Records;

public record Reward(uint Index, Item Item, uint StackSize, Item RequiredItem, uint RequiredCount);
