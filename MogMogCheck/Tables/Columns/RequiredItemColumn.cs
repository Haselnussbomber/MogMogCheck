using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Gui.ImGuiTable;
using HaselCommon.Services;
using HaselCommon.Utils;
using MogMogCheck.Caches;
using MogMogCheck.Records;

namespace MogMogCheck.Tables;

// TODO: add some kind of filter logic

[RegisterSingleton, AutoConstruct]
public partial class RequiredItemColumn : ColumnNumber<ShopItem>
{
    private readonly ITextureProvider _textureProvider;
    private readonly ItemQuantityService _itemQuantityService;
    private readonly ItemService _itemService;

    [AutoPostConstruct]
    private void Initialize()
    {
        SetFixedWidth(130);
    }

    public override int ToValue(ShopItem row)
        => (int)row.GiveItems[0].Amount;

    public override void DrawColumn(ShopItem row)
    {
        ImCursor.Y += MathF.Round(ImStyle.FramePadding.Y / 2f); // my cell padding

        var giveItemCount = row.GiveItems.Count(tuple => !tuple.Item.IsEmpty && tuple.Amount > 0);

        for (var i = 0; i < giveItemCount; i++)
        {
            using var id = ImRaii.PushId(i);

            var (item, amount) = row.GiveItems[i];
            var hasEnough = _itemQuantityService.Get(item) >= amount;

            _textureProvider.DrawIcon(_itemService.GetItemIcon(item), new DrawInfo(ImStyle.FrameHeight)
            {
                TintColor = hasEnough ? null : Color.Text700.ToVector()
            });

            ImGuiContextMenu.Draw("RequiredItemColumnContextMenu", builder =>
            {
                builder.AddItemFinder(item);
                builder.AddLinkItem(item);
                builder.AddCopyItemName(item);
                builder.AddOpenOnGarlandTools("item", item);
            });

            ImGui.SameLine(0, ImStyle.ItemInnerSpacing.X);

            using (ImRaii.Disabled(!hasEnough))
                ImGui.Text(amount.ToString());

            var it = new IterationArgs(i, giveItemCount);
            if (!it.IsLast)
                ImGui.SameLine();
        }
    }
}
