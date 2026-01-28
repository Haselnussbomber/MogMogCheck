using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Services;
using HaselCommon.Services.Commands;
using Microsoft.Extensions.Hosting;
using MogMogCheck.Windows;

namespace MogMogCheck.Services;

[RegisterSingleton<IHostedService>(Duplicate = DuplicateStrategy.Append), AutoConstruct]
public partial class PluginCommandService : IHostedService
{
    private readonly IDalamudPluginInterface _pluginInterface;
    private readonly WindowManager _windowManager;
    private readonly CommandService _commandService;
    private readonly IClientState _clientState;

    private bool _mainUiHandlerRegistered;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _commandService.AddCommand("mogmog", cmd =>
        {
            cmd.WithHelpTextKey("MogMogCheck.CommandHandlerHelpMessage");
            cmd.WithHandler(OnMainCommand);

            cmd.AddSubcommand("config", cmd =>
            {
                cmd.WithHelpTextKey("MogMogCheck.CommandHandler.Config.HelpMessage");
                cmd.WithHandler(OnConfigCommand);
            });

#if DEBUG
            cmd.AddSubcommand("debug", cmd => cmd.WithHandler(OnDebugCommand));
#endif
        });

        _pluginInterface.UiBuilder.OpenConfigUi += ToggleConfigWindow;

        _clientState.Login += OnLogin;
        _clientState.Logout += OnLogout;

        if (_clientState.IsLoggedIn)
            EnableMainUiHandler();

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        DisableMainUiHandler();

        _pluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigWindow;

        _clientState.Login -= OnLogin;
        _clientState.Logout -= OnLogout;

        return Task.CompletedTask;
    }

    private void OnLogin()
    {
        EnableMainUiHandler();
    }

    private void OnLogout(int type, int code)
    {
        DisableMainUiHandler();
    }

    private void OnMainCommand(CommandContext ctx)
    {
        ToggleMainWindow();
    }

    private void OnConfigCommand(CommandContext ctx)
    {
        ToggleConfigWindow();
    }

    private void OnDebugCommand(CommandContext context)
    {
        ToggleDebugWindow();
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

    private void ToggleMainWindow()
    {
        _windowManager.CreateOrToggle<MainWindow>();
    }

    private void ToggleConfigWindow()
    {
        _windowManager.CreateOrToggle<ConfigWindow>();
    }

    private void ToggleDebugWindow()
    {
        _windowManager.CreateOrToggle<DebugWindow>();
    }
}
