using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using Microsoft.Extensions.Hosting;
using MogMogCheck.Caches;
using MogMogCheck.Config;

namespace MogMogCheck.Services;

[RegisterSingleton, RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class AutoUntrackService : IHostedService
{
    private readonly IClientState _clientState;
    private readonly IGameInventory _gameInventory;
    private readonly PluginConfig _pluginConfig;
    private readonly SpecialShopService _specialShopService;
    private readonly ItemQuantityService _itemQuantityService;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _gameInventory.InventoryChangedRaw += GameInventory_InventoryChangedRaw;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _gameInventory.InventoryChangedRaw -= GameInventory_InventoryChangedRaw;
        return Task.CompletedTask;
    }

    private void GameInventory_InventoryChangedRaw(IReadOnlyCollection<InventoryEventArgs> events)
    {
        Check();
    }

    public void Check()
    {
        if (!_clientState.IsLoggedIn || !_specialShopService.HasData || !_pluginConfig.CheckboxMode || !_pluginConfig.AutoUntrack)
            return;

        if (_pluginConfig.TrackedItems.RemoveAll((uint itemId, uint amount) => amount == 1 && _itemQuantityService.TryGetValue(itemId, out var quantity) && quantity != 0))
            _pluginConfig.Save();
    }
}
