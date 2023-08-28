using HaselCommon.Sheets;
using ContentFinderCondition = Lumina.Excel.GeneratedSheets.ContentFinderCondition;

namespace MogMogCheck.Records;

public record Duty(ContentFinderCondition ContentFinderCondition, Item RewardItem, uint RewardItemCount, uint RewardItemCountLoss);
