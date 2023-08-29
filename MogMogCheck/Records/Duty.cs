using HaselCommon.Sheets;
using ContentFinderCondition = Lumina.Excel.GeneratedSheets.ContentFinderCondition;
using InstanceContent = Lumina.Excel.GeneratedSheets.InstanceContent;
using InstanceContentCSBonus = Lumina.Excel.GeneratedSheets.InstanceContentCSBonus;

namespace MogMogCheck.Records;

public record Duty
{
    public Duty(InstanceContentCSBonus row)
    {
        var instanceContent = GetRow<InstanceContent>(row.Instance.Row);
        ContentFinderCondition = instanceContent != null ? FindRow<ContentFinderCondition>(row => row?.ContentLinkType == 1 && row.Content == instanceContent.RowId) : null;
        RewardItem = row.Item.Row != 0 ? GetRow<Item>(row.Item.Row) : null;
        RewardItemCount = row.Unknown2;
        RewardItemCountLoss = row.Unknown3;
    }

    public ContentFinderCondition? ContentFinderCondition { get; }
    public Item? RewardItem { get; }
    public uint RewardItemCount { get; }
    public uint RewardItemCountLoss { get; }
}
