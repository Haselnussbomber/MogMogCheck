using System.Threading;
using System.Threading.Tasks;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using HaselCommon.Services;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.Hosting;
using MogMogCheck.Config;
using MogMogCheck.Windows;

namespace MogMogCheck.Services;

[RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class PluginWindowService : IHostedService
{
    private readonly WindowManager _windowManager;
    private readonly IAddonLifecycle _addonLifecycle;
    private readonly PluginConfig _pluginConfig;
    private readonly ExcelService _excelService;
    private readonly SpecialShopService _specialShopService;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _addonLifecycle.RegisterListener(AddonEvent.PostShow, "MoogleCollection", OnMoogleCollectionShow);
        _addonLifecycle.RegisterListener(AddonEvent.PostShow, "ShopExchangeItem", OnShopExchangeItemShow);
        _addonLifecycle.RegisterListener(AddonEvent.PreHide, "MoogleCollection", OnMoogleCollectionHide);
        _addonLifecycle.RegisterListener(AddonEvent.PreHide, "ShopExchangeItem", OnShopExchangeItemHide);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _addonLifecycle.UnregisterListener(AddonEvent.PostShow, "MoogleCollection", OnMoogleCollectionShow);
        _addonLifecycle.UnregisterListener(AddonEvent.PostShow, "ShopExchangeItem", OnShopExchangeItemShow);
        _addonLifecycle.UnregisterListener(AddonEvent.PreHide, "MoogleCollection", OnMoogleCollectionHide);
        _addonLifecycle.UnregisterListener(AddonEvent.PreHide, "ShopExchangeItem", OnShopExchangeItemHide);

        return Task.CompletedTask;
    }

    private void OnMoogleCollectionShow(AddonEvent type, AddonArgs args)
    {
        if (!_pluginConfig.OpenWithMogpendium)
            return;

        var window = _windowManager.CreateOrOpen<MainWindow>(false);
        window.DisableWindowSounds = true;
    }

    private unsafe void OnShopExchangeItemShow(AddonEvent type, AddonArgs args)
    {
        if (!_pluginConfig.OpenWithShop)
            return;

        // Find the SpecialShopEventHandler the LocalPlayer is interacting with
        // and check if it's one of the SpecialShops the Itinerant moogle provides.
        // TODO: find a better way lol

        var localPlayer = Control.GetLocalPlayer();
        if (localPlayer == null)
            return;

        if (!_excelService.TryGetSubrows<CustomTalkNestHandlers>(720897, out var handlers))
            return;

        foreach (var eventHandler in EventFramework.Instance()->EventHandlerModule.EventHandlerMap.Values)
        {
            if (eventHandler.Value == null || eventHandler.Value->Info.EventId.ContentId != EventHandlerContent.SpecialShop)
                continue;

            foreach (var eventObject in eventHandler.Value->EventObjects)
            {
                if (eventObject.Value == null || eventObject.Value != localPlayer)
                    continue;

                foreach (var handler in handlers)
                {
                    var shopId = eventHandler.Value->Info.EventId.Id;

                    if (handler.NestHandler.RowId != shopId)
                        continue;

                    if (_specialShopService.HasData && _specialShopService.ShopId != shopId)
                        continue;

                    var window = _windowManager.CreateOrOpen<MainWindow>(false);
                    window.DisableWindowSounds = true;
                    break;
                }

                break;
            }
        }
    }

    private void OnMoogleCollectionHide(AddonEvent type, AddonArgs args)
    {
        if (!_pluginConfig.OpenWithMogpendium)
            return;

        _windowManager.Close<MainWindow>();
    }

    private void OnShopExchangeItemHide(AddonEvent type, AddonArgs args)
    {
        if (!_pluginConfig.OpenWithShop)
            return;

        _windowManager.Close<MainWindow>();
    }
}
