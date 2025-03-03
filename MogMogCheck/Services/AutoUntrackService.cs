using System.Collections.Generic;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions.Collections;
using MogMogCheck.Caches;
using MogMogCheck.Config;

namespace MogMogCheck.Services;

[RegisterSingleton, AutoConstruct]
public partial class AutoUntrackService : IDisposable
{
    private readonly IClientState _clientState;
    private readonly IGameInventory _gameInventory;
    private readonly PluginConfig _pluginConfig;
    private readonly SpecialShopService _specialShopService;
    private readonly ItemQuantityCache _itemQuantityCache;

    [AutoPostConstruct]
    private void Initialize()
    {
        _gameInventory.InventoryChangedRaw += GameInventory_InventoryChangedRaw;
    }

    public void Dispose()
    {
        _gameInventory.InventoryChangedRaw -= GameInventory_InventoryChangedRaw;
    }

    private void GameInventory_InventoryChangedRaw(IReadOnlyCollection<InventoryEventArgs> events)
    {
        if (!_clientState.IsLoggedIn || !_specialShopService.HasData || !_pluginConfig.CheckboxMode || !_pluginConfig.AutoUntrack)
            return;

        if (_pluginConfig.TrackedItems.RemoveAll((uint itemId, uint amount) => amount == 1 && _itemQuantityCache.TryGetValue(itemId, out var quantity) && quantity != 0))
            _pluginConfig.Save();
    }
}
