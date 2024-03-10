using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using HaselCommon.Sheets;

namespace MogMogCheck.Records;

public record TrackedItem
{
    private uint lastUpdateFrame;
    private uint quantity;

    public TrackedItem(ExtendedItem Item, uint Quantity)
    {
        quantity = Quantity;
        this.Item = Item;
    }

    public ExtendedItem Item { get; }
    public unsafe uint Quantity
    {
        get
        {
            var currentFrame = Framework.Instance()->FrameCounter;
            if (lastUpdateFrame == currentFrame)
                return quantity;

            quantity = (uint)InventoryManager.Instance()->GetInventoryItemCount(Item.RowId);
            lastUpdateFrame = currentFrame;

            return quantity;
        }
    }
}
