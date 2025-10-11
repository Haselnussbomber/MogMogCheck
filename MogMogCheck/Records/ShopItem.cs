using System.Collections.Generic;
using HaselCommon.Utils;

namespace MogMogCheck.Records;

public record struct ShopItem(int Index, IReadOnlyList<ItemAmount> ReceiveItems, IReadOnlyList<ItemAmount> GiveItems);
