using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using HaselCommon.Utils;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MogMogCheck.Caches;
using MogMogCheck.Config;

namespace MogMogCheck.Services;

[RegisterSingleton, RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class AutoUntrackService : IHostedService
{
    private readonly ILogger<AutoUntrackService> _logger;
    private readonly IClientState _clientState;
    private readonly PluginConfig _pluginConfig;
    private readonly SpecialShopService _specialShopService;
    private readonly ItemQuantityService _itemQuantityService;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _itemQuantityService.Cleared += OnItemQuantityCleared;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _itemQuantityService.Cleared -= OnItemQuantityCleared;
        return Task.CompletedTask;
    }

    private void OnItemQuantityCleared()
    {
        Check();
    }

    public void Check()
    {
        if (!_clientState.IsLoggedIn || !_specialShopService.HasData || !_pluginConfig.CheckboxMode || !_pluginConfig.AutoUntrack)
            return;

        var anyRemoved = false;

        foreach (var itemId in _pluginConfig.TrackedItems.Keys.ToArray())
        {
            if (!_pluginConfig.TrackedItems.TryGetValue(itemId, out var amount))
                continue;

            if (amount == 1 && _itemQuantityService.Get(itemId) is { } quantity && quantity != 0)
            {
                _logger.LogDebug("Untracking item #{itemId} ({name})", itemId, ((ItemHandle)itemId).Name.ToString());
                anyRemoved |= _pluginConfig.TrackedItems.Remove(itemId);
            }
        }

        if (anyRemoved)
        {
            _pluginConfig.Save();
        }
    }
}
