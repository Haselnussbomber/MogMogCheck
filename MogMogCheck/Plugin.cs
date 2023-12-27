using Dalamud.Plugin;
using HaselCommon.Extensions;
using MogMogCheck.Windows;

namespace MogMogCheck;

public sealed partial class Plugin : IDalamudPlugin
{
    public static Configuration Config { get; private set; } = null!;

    public unsafe Plugin(DalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);

        Config = Configuration.Load();
        Config.TrackedItems.RemoveAll((uint itemId, uint amount) => amount == 0); // clear old untracked items

        Service.TranslationManager.Initialize(Config);

        Service.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        // Service.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

        Service.TranslationManager.OnLanguageChange += Config.Save;

        Service.CommandManager.AddHandler("/mogmog", new(OnCommand));
    }

    private void OnCommand(string command, string arguments)
    {
        Service.WindowManager.ToggleWindow<MainWindow>();
    }

    void IDisposable.Dispose()
    {
        Service.CommandManager.RemoveHandler("/mogmog");

        Service.PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        // Service.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;

        Service.TranslationManager.OnLanguageChange -= Config.Save;

        Config?.Save();
        Config = null!;
        Service.Dispose();
    }

    private void OpenMainUi()
    {
        Service.WindowManager.ToggleWindow<MainWindow>();
    }

    // private void OpenConfigUi()
    // {
    //     Service.WindowManager.ToggleWindow<ConfigWindow>();
    // }
}
