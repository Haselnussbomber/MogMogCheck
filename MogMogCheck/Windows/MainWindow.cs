using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using HaselCommon.Gui;
using HaselCommon.Services;
using HaselCommon.Windows;
using MogMogCheck.Caches;
using MogMogCheck.Config;
using MogMogCheck.Services;
using MogMogCheck.Tables;

namespace MogMogCheck.Windows;

[RegisterSingleton, AutoConstruct]
public partial class MainWindow : SimpleWindow
{
    private readonly WindowManager _windowManager;
    private readonly IClientState _clientState;
    private readonly TextService _textService;
    private readonly ItemService _itemService;
    private readonly ITextureProvider _textureProvider;
    private readonly ItemQuantityService _itemQuantityService;
    private readonly PluginConfig _pluginConfig;
    private readonly SpecialShopService _specialShopService;
    private readonly AutoUntrackService _autoUntrackService;
    private readonly ShopItemTable _table;
    private bool _hasClearedUntrackedItems;
    private HashSet<uint> _processedItems = [];

    private bool IsConfigWindowOpen => _windowManager.TryGetWindow<ConfigWindow>(out var configWindow) && configWindow.IsOpen;

    [AutoPostConstruct]
    private void Initialize()
    {
        Size = new Vector2(570, 740);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(350, 68),
            MaximumSize = new Vector2(4069),
        };

        Flags |= ImGuiWindowFlags.NoScrollbar;

        TitleBarButtons.Add(new()
        {
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new(0, 1),
            ShowTooltip = () =>
            {
                using var tooltip = ImRaii.Tooltip();
                ImGui.Text(_textService.Translate(IsConfigWindowOpen
                    ? "TitleBarButton.ToggleConfig.Tooltip.CloseConfig"
                    : "TitleBarButton.ToggleConfig.Tooltip.OpenConfig"));
            },
            Click = (button) => _windowManager.CreateOrToggle<ConfigWindow>()
        });
    }

    public override void PreDraw()
    {
        base.PreDraw();

        if (!_hasClearedUntrackedItems && _specialShopService.HasData)
        {
            // clear old untracked items
            if (_pluginConfig.TrackedItems.RemoveAll((uint itemId, uint amount) => amount == 0 || !_specialShopService.ShopItems.Any(entry => entry.ReceiveItems.Any(ri => ri.Item.ItemId == itemId))))
                _pluginConfig.Save();

            _autoUntrackService.Check();

            _hasClearedUntrackedItems = true;
        }
    }

    public override void OnOpen()
    {
        _itemQuantityService.Clear();
        base.OnOpen();
    }

    public override void OnClose()
    {
        DisableWindowSounds = false;
        base.OnClose();
    }

    public override bool DrawConditions()
    {
        return _clientState.IsLoggedIn && base.DrawConditions();
    }

    public override void Draw()
    {
        if (!_specialShopService.HasData)
        {
            // The Moogle Treasure Trove is not currently underway.

            foreach (var line in _textService.GetAddonText(15909).Split("\n"))
            {
                ImGuiHelpers.CenteredText(line);
            }

            return;
        }

        DrawTomestoneCount(_specialShopService.CurrencyItem1);
        if (_specialShopService.CurrencyItem2 != 0)
        {
            ImGui.SameLine();
            DrawTomestoneCount(_specialShopService.CurrencyItem2);
        }
        ImCursor.Y += 1;

        _table.Draw();
    }

    private void DrawTomestoneCount(uint itemId)
    {
        var startY = ImCursor.Y;

        _textureProvider.DrawIcon(_itemService.GetItemIcon(itemId), 32 * ImStyle.Scale);

        ImGuiContextMenu.Draw("TomestoneItemContextMenu" + itemId.ToString(), builder =>
        {
            builder.AddItemFinder(itemId);
            builder.AddLinkItem(itemId);
            builder.AddCopyItemName(itemId);
            builder.AddOpenOnGarlandTools("item", itemId);
        });

        ImGui.SameLine(0, ImCursor.X);
        ImCursor.Y = startY + 16 * ImStyle.Scale / 2f - 1.5f; // idk lol

        var needed = 0u;
        _processedItems.Clear();
        foreach (var item in _specialShopService.ShopItems)
        {
            var recieveItem = item.ReceiveItems[0].Item;

            if (!_processedItems.Add(recieveItem))
                continue;

            if (!_pluginConfig.TrackedItems.TryGetValue(recieveItem, out var count))
                continue;

            foreach (var giveItem in item.GiveItems)
            {
                if (giveItem.Item.ItemId == itemId)
                {
                    needed += giveItem.Amount * count;
                    break;
                }
            }
        }

        var quantity = _itemQuantityService.Get(itemId);
        if (needed > quantity)
        {
            var remaining = needed - quantity;
            ImGui.Text(_textService.Translate("Currency.InfoWithRemaining", quantity, needed, remaining));
        }
        else
        {
            ImGui.Text(_textService.Translate("Currency.Info", quantity, needed));
        }
    }
}
