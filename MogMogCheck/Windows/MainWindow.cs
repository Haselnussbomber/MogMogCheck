using System.Collections.Generic;
using System.Numerics;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using HaselCommon;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
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
    private readonly ItemQuantityCache _itemQuantityCache;
    private readonly TextureService _textureService;
    private readonly ImGuiContextMenuService _imGuiContextMenuService;
    private readonly PluginConfig _pluginConfig;
    private readonly SpecialShopService _specialShopService;
    private readonly ItemService _itemService;
    private readonly ShopItemTable _table;

    private bool IsConfigWindowOpen => _windowManager.TryGetWindow<ConfigWindow>(out var configWindow) && configWindow.IsOpen;

    [AutoPostConstruct]
    private void Initialize()
    {
        Size = new Vector2(570, 740);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(300, 200),
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
                ImGui.TextUnformatted(_textService.Translate(IsConfigWindowOpen
                    ? "TitleBarButton.ToggleConfig.Tooltip.CloseConfig"
                    : "TitleBarButton.ToggleConfig.Tooltip.OpenConfig"));
            },
            Click = (button) => _windowManager.CreateOrToggle(Service.Get<ConfigWindow>)
        });

        _gameInventory.InventoryChangedRaw += OnInventoryChanged;
    }

    public override void Dispose()
    {
        _gameInventory.InventoryChangedRaw -= OnInventoryChanged;
        base.Dispose();
        GC.SuppressFinalize(this);
    }

    private void OnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        _itemQuantityCache.Clear();
    }

    public override bool DrawConditions()
        => _clientState.IsLoggedIn
            && _specialShopService.CurrentSeason != null
            && _specialShopService.CurrentShop != null
            && _specialShopService.ShopItems.Length != 0
            && _specialShopService.TomestoneItemIds.Length != 0;

    public override void Draw()
    {
        if (!_specialShopService.CurrentShop.HasValue)
        {
            ImGui.TextUnformatted("Shop not open.");
            return;
        }

        DrawTomestoneCount();
        _table.Draw();
    }

    private void DrawTomestoneCount()
    {
        var items = _specialShopService.ShopItems;

        var scale = ImGuiHelpers.GlobalScale;

        foreach (var tomestoneItemId in _specialShopService.TomestoneItemIds)
        {
            _textureService.DrawIcon(_itemService.GetIconId(tomestoneItemId), 32 * scale);

            _imGuiContextMenuService.Draw($"##Tomestone_ItemContextMenu{tomestoneItemId}", builder =>
            {
                builder.AddItemFinder(tomestoneItemId);
                builder.AddCopyItemName(tomestoneItemId);
                builder.AddItemSearch(tomestoneItemId);
                builder.AddOpenOnGarlandTools("item", tomestoneItemId);
            });

            ImGui.SameLine(45 * scale);
            ImGuiUtils.PushCursorY(6 * scale);

            var needed = 0u;
            for (var i = 0; i < items.Length; i++)
            {
                needed += _pluginConfig.TrackedItems.TryGetValue(items[i].ReceiveItems[0].ItemId, out var amount) ? amount * items[i].GiveItems[0].Quantity : 0u;
            }

            var quantity = _itemQuantityCache.GetValue(tomestoneItemId);
            if (needed > quantity)
            {
                var remaining = needed - quantity;
                ImGui.TextUnformatted(_textService.Translate("Currency.InfoWithRemaining", quantity, needed, remaining));
            }
            else
            {
                ImGui.TextUnformatted(_textService.Translate("Currency.Info", quantity, needed));
            }
        }
    }
}
