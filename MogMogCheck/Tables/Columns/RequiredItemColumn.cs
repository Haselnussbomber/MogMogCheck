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
    private readonly ItemQuantityCache _itemQuantityCache;
    private readonly ImGuiContextMenuService _imGuiContextMenuService;

    [AutoPostConstruct]
    private void Initialize()
    {
        SetFixedWidth(130);
    }

    public override int ToValue(ShopItem row)
        => (int)row.GiveItems[0].Amount;

    public override void DrawColumn(ShopItem row)
    {
        ImGuiUtils.PushCursorY(MathF.Round(ImGui.GetStyle().FramePadding.Y / 2f)); // my cell padding

        // TODO: add support for items 2 and 3 whenever it becomes necessary
        var (item, amount) = row.GiveItems[0];

        var hasEnoughTomestones = _itemQuantityCache.GetValue(item) >= amount;

        _textureProvider.DrawIcon(item.Icon, new DrawInfo(ImGui.GetFrameHeight())
        {
            TintColor = hasEnoughTomestones ? null : (Vector4)Color.Grey
        });

        _imGuiContextMenuService.Draw($"##RequiredItemColumnContextMenu{item}", builder =>
        {
            builder.AddItemFinder(item);
            builder.AddLinkItem(item);
            builder.AddCopyItemName(item);
            builder.AddOpenOnGarlandTools("item", item);
        });

        ImGui.SameLine();

        using (ImRaii.Disabled(!hasEnoughTomestones))
            ImGui.Text(amount.ToString());
    }
}
