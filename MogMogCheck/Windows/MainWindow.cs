using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
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
public unsafe partial class MainWindow : SimpleWindow
{
    private readonly WindowManager _windowManager;
    private readonly IGameInventory _gameInventory;
    private readonly IClientState _clientState;
    private readonly TextService _textService;
    private readonly ITextureProvider _textureProvider;
    private readonly ItemQuantityCache _itemQuantityCache;
    private readonly ImGuiContextMenuService _imGuiContextMenuService;
    private readonly PluginConfig _pluginConfig;
    private readonly SpecialShopService _specialShopService;
    private readonly AutoUntrackService _autoUntrackService;
    private readonly ShopItemTable _table;
    private bool _hasClearedUntrackedItems;

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

        _gameInventory.InventoryChangedRaw += OnInventoryChanged;
    }

    public override void Dispose()
    {
        _gameInventory.InventoryChangedRaw -= OnInventoryChanged;
        base.Dispose();
    }

    private void OnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        _itemQuantityCache.Clear();
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
        _itemQuantityCache.Clear();
        base.OnOpen();
    }

    public override void OnClose()
    {
        DisableWindowSounds = false;
        base.OnClose();
    }

    public override bool DrawConditions()
        => _clientState.IsLoggedIn;

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

        DrawTomestoneCount();
        _table.Draw();
    }

    private void DrawTomestoneCount()
    {
        var scale = ImGuiHelpers.GlobalScale;
        var items = _specialShopService.ShopItems;
        var tomestoneItem = _specialShopService.TomestoneItem;

        _textureProvider.DrawIcon(tomestoneItem.Icon, 32 * scale);

        _imGuiContextMenuService.Draw("##Tomestone_ItemContextMenu", builder =>
        {
            builder.AddItemFinder(tomestoneItem);
            builder.AddLinkItem(tomestoneItem);
            builder.AddCopyItemName(tomestoneItem);
            builder.AddOpenOnGarlandTools("item", tomestoneItem);
        });

        ImGui.SameLine(45 * scale);
        ImGuiUtils.PushCursorY(6 * scale);

        var needed = 0u;
        for (var i = 0; i < items.Count; i++)
        {
            needed += _pluginConfig.TrackedItems.TryGetValue(items[i].ReceiveItems[0].Item, out var amount) ? amount * items[i].GiveItems[0].Amount : 0u;
        }

        var quantity = _itemQuantityCache.GetValue(tomestoneItem);
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
