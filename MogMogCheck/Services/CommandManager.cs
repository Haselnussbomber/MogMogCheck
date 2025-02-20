using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon;
using HaselCommon.Services;
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

    private void AddonObserver_AddonOpen(string addonName)
    {
        if (_pluginConfig.OpenWithMogpendium && addonName == "MoogleCollection")
        {
            var window = _windowManager.CreateOrOpen(Service.Get<MainWindow>);
            window.DisableWindowSounds = true;
        }
    }

    private void AddonObserver_AddonClose(string addonName)
    {
        if (_pluginConfig.OpenWithMogpendium && addonName == "MoogleCollection")
        {
            _windowManager.Close<MainWindow>();
        }
    }

    private void ToggleMainWindow()
    {
        _windowManager.CreateOrToggle(Service.Get<MainWindow>);
    }

    private void ToggleConfigWindow()
    {
        _windowManager.CreateOrToggle(Service.Get<ConfigWindow>);
    }
}
