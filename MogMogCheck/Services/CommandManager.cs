using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using HaselCommon.Services;
using Lumina.Excel.Sheets;
using MogMogCheck.Config;
using MogMogCheck.Windows;

namespace MogMogCheck.Services;

[RegisterSingleton, AutoConstruct]
public partial class CommandManager : IDisposable
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly WindowManager _windowManager;
    private readonly CommandService _commandService;
    private readonly IClientState _clientState;
    private readonly PluginConfig _pluginConfig;
    private readonly AddonObserver _addonObserver;
    private readonly ExcelService _excelService;
    private readonly SpecialShopService _specialShopService;
    private bool _mainUiHandlerRegistered;

    [AutoPostConstruct]
    private void Initialize()
    {
        _commandService.Register("/mogmog", "CommandHandlerHelpMessage", HandleCommand, autoEnable: true);

        _pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigWindow;
        if (_clientState.IsLoggedIn) EnableMainUiHandler();

        _clientState.Login += OnLogin;
        _clientState.Logout += OnLogout;

        _addonObserver.AddonOpen += AddonObserver_AddonOpen;
        _addonObserver.AddonClose += AddonObserver_AddonClose;
    }

    public void Dispose()
    {
        DisableMainUiHandler();

        _pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigWindow;

        _clientState.Login -= OnLogin;
        _clientState.Logout -= OnLogout;

        _addonObserver.AddonOpen -= AddonObserver_AddonOpen;
        _addonObserver.AddonClose -= AddonObserver_AddonClose;

        GC.SuppressFinalize(this);
    }

    private void OnLogin()
    {
        EnableMainUiHandler();
    }

    private void OnLogout(int type, int code)
    {
        DisableMainUiHandler();
    }

    private void EnableMainUiHandler()
    {
        if (!_mainUiHandlerRegistered)
        {
            _pluginInterface.UiBuilder.OpenMainUi += ToggleMainWindow;
            _mainUiHandlerRegistered = true;
        }
    }

    private void DisableMainUiHandler()
    {
        if (_mainUiHandlerRegistered)
        {
            _pluginInterface.UiBuilder.OpenMainUi -= ToggleMainWindow;
            _mainUiHandlerRegistered = false;
        }
    }

    private void HandleCommand(string command, string arguments)
    {
        switch (arguments)
        {
            case "config":
                ToggleConfigWindow();
                break;

            default:
                ToggleMainWindow();
                break;
        }
    }

    private unsafe void AddonObserver_AddonOpen(string addonName)
    {
        if (_pluginConfig.OpenWithMogpendium && addonName == "MoogleCollection")
        {
            var window = _windowManager.CreateOrOpen<MainWindow>(false);
            window.DisableWindowSounds = true;
        }
        else if (_pluginConfig.OpenWithShop && addonName == "ShopExchangeItem")
        {
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
    }

    private void AddonObserver_AddonClose(string addonName)
    {
        if (_pluginConfig.OpenWithMogpendium && addonName == "MoogleCollection")
        {
            _windowManager.Close<MainWindow>();
        }
        else if (_pluginConfig.OpenWithShop && addonName == "ShopExchangeItem")
        {
            _windowManager.Close<MainWindow>();
        }
    }

    private void ToggleMainWindow()
    {
        _windowManager.CreateOrToggle<MainWindow>();
    }

    private void ToggleConfigWindow()
    {
        _windowManager.CreateOrToggle<ConfigWindow>();
    }
}
