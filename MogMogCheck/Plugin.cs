using Dalamud.Game.Command;
using Dalamud.Plugin;
using MogMogCheck.Windows;

namespace MogMogCheck;

public sealed class Plugin : IDalamudPlugin
{
    private readonly CommandInfo CommandInfo;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);
        Service.AddService(Configuration.Load());

        Service.PluginInterface.LanguageChanged += PluginInterface_LanguageChanged;
        Service.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
        Service.AddonObserver.AddonOpen += AddonObserver_AddonOpen;
        Service.AddonObserver.AddonClose += AddonObserver_AddonClose;

        CommandInfo = new CommandInfo(OnCommand) { HelpMessage = t("CommandHandlerHelpMessage") };

        Service.CommandManager.AddHandler("/mogmog", CommandInfo);
    }

    private void PluginInterface_LanguageChanged(string langCode)
    {
        CommandInfo.HelpMessage = t("CommandHandlerHelpMessage");
    }

    private void OpenMainUi()
    {
        Service.WindowManager.ToggleWindow<MainWindow>();
    }

    private void OpenConfigUi()
    {
        Service.WindowManager.ToggleWindow<ConfigWindow>();
    }

    private void OnCommand(string command, string arguments)
    {
        switch (arguments.ToLower())
        {
#if DEBUG
            case "debug":
                Service.WindowManager.ToggleWindow<DebugWindow>();
                break;
#endif

            case "config":
                Service.WindowManager.ToggleWindow<ConfigWindow>();
                break;

            default:
                Service.WindowManager.ToggleWindow<MainWindow>();
                break;
        }
    }

    private void AddonObserver_AddonOpen(string addonName)
    {
        if (Service.GetService<Configuration>().OpenWithMogpendium && addonName == "MoogleCollection")
        {
            Service.WindowManager.OpenWindow<MainWindow>().DisableWindowSounds = true;
        }
    }

    private void AddonObserver_AddonClose(string addonName)
    {
        if (Service.GetService<Configuration>().OpenWithMogpendium && addonName == "MoogleCollection")
        {
            Service.WindowManager.CloseWindow<MainWindow>();
        }
    }

    void IDisposable.Dispose()
    {
        Service.CommandManager.RemoveHandler("/mogmog");

        Service.PluginInterface.LanguageChanged -= PluginInterface_LanguageChanged;
        Service.PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        Service.AddonObserver.AddonOpen -= AddonObserver_AddonOpen;
        Service.AddonObserver.AddonClose -= AddonObserver_AddonClose;

        Service.Dispose();
    }
}
