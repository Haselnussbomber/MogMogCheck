using HaselCommon.Utils;
using Lumina.Excel.Sheets;

namespace MogMogCheck.Records;

public record struct ItemEntry(ExcelRowId<Item> ItemId, uint Quantity);
