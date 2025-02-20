using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Gui.ImGuiTable;
using HaselCommon.Services;
using ImGuiNET;
using MogMogCheck.Caches;
using MogMogCheck.Records;
using MogMogCheck.Services;

namespace MogMogCheck.Tables;

// TODO: add some kind of filter logic

[RegisterSingleton, AutoConstruct]
public partial class RequiredItemColumn : ColumnNumber<ShopItem>
{
    private readonly ItemQuantityCache _itemQuantityCache;
    private readonly ItemService _itemService;
    private readonly TextureService _textureService;
    private readonly ImGuiContextMenuService _imGuiContextMenuService;
    private readonly SpecialShopService _specialShopService;

    [AutoPostConstruct]
    private void Initialize()
    {
        SetFixedWidth(130);
    }

    public override int ToValue(ShopItem row)
        => (int)row.GiveItems[0].Quantity;

    public override void DrawColumn(ShopItem row)
    {
        ImGuiUtils.PushCursorY(MathF.Round(ImGui.GetStyle().FramePadding.Y / 2f)); // my cell padding

        // TODO: add support for items 2 and 3 whenever it becomes necessary
        var (itemId, quantity) = row.GiveItems[0];

        var hasEnoughTomestones = _specialShopService.TomestoneItemIds.Contains((uint)itemId) && _itemQuantityCache.GetValue(itemId) >= quantity;

        _textureService.DrawIcon(_itemService.GetIconId(itemId), new DrawInfo(ImGui.GetFrameHeight())
        {
            TintColor = hasEnoughTomestones ? null : (Vector4)Color.Grey
        });

        _imGuiContextMenuService.Draw($"##RequiredItemColumnContextMenu{itemId}", builder =>
        {
            builder.AddItemFinder(itemId);
            builder.AddCopyItemName(itemId);
            builder.AddItemSearch(itemId);
            builder.AddOpenOnGarlandTools("item", itemId);
        });

        ImGui.SameLine();

        using (ImRaii.Disabled(!hasEnoughTomestones))
            ImGui.TextUnformatted(quantity.ToString());
    }
}
