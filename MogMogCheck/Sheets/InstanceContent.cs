using Lumina;
using Lumina.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace MogMogCheck.Sheets;

public class InstanceContent : Lumina.Excel.GeneratedSheets.InstanceContent
{
    public LazyRow<ContentFinderCondition> ContentFinderCondition { get; set; } = null!;

    public override void PopulateData(RowParser parser, GameData gameData, Language language)
    {
        base.PopulateData(parser, gameData, language);

        ContentFinderCondition = new(gameData, FindRow<ContentFinderCondition>(row => row?.ContentLinkType == 1 && row.Content == RowId)?.RowId ?? 0, language);
    }
}
