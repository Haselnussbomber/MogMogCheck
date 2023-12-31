using HaselCommon.Sheets;
using Lumina.Excel.GeneratedSheets;

namespace MogMogCheck.Records;

public record Duty
{
    public Duty(InstanceContentCSBonus row)
    {
        var instanceContent = GetRow<InstanceContent>(row.Instance.Row);
        ContentFinderCondition = instanceContent != null ? FindRow<ContentFinderCondition>(row => row?.ContentLinkType == 1 && row.Content == instanceContent.RowId) : null;
        RewardItem = row.Item.Row != 0 ? GetRow<ExtendedItem>(row.Item.Row) : null;
        RewardItemCount = row.Unknown2;
        RewardItemCountLoss = row.Unknown3;
        RewardItemCountTie = row.Unknown4;
    }

    public ContentFinderCondition? ContentFinderCondition { get; }
    public ExtendedItem? RewardItem { get; }
    public uint RewardItemCount { get; }
    public uint RewardItemCountLoss { get; }
    public uint RewardItemCountTie { get; }
}
