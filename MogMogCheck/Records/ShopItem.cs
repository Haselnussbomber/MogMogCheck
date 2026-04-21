using System.Collections.Generic;
using HaselCommon.Utils;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace MogMogCheck.Records;

public record struct ShopItem(
    int Index,
    IReadOnlyList<ItemAmount> ReceiveItems,
    IReadOnlyList<ItemAmount> GiveItems,
    RowRef<Quest> RequiredQuest);
