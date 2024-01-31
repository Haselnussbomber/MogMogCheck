using Dalamud.Interface.GameFonts;
using Dalamud.Game.Command;
using Dalamud.Plugin;
using MogMogCheck.Windows;

namespace MogMogCheck;

public sealed class Plugin : IDalamudPlugin
{
    public static Configuration Config { get; private set; } = null!;
    private static GameFontHandle? TripleTriadNumberFont = null!;
    private static float TripleTriadNumberFontSize = 0;

    private readonly CommandInfo CommandInfo;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);
        Config = Configuration.Load();

        CommandInfo = new CommandInfo(OnCommand) { HelpMessage = t("CommandHandlerHelpMessage") };

        Service.Framework.RunOnFrameworkThread(Setup);
    }

    private void Setup()
    {
        HaselCommon.Interop.Resolver.GetInstance.Resolve();

        Service.PluginInterface.LanguageChanged += PluginInterface_LanguageChanged;
        Service.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
        Service.AddonObserver.AddonOpen += AddonObserver_AddonOpen;
        Service.AddonObserver.AddonClose += AddonObserver_AddonClose;

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
        if (Config.OpenWithMogpendium && addonName == "MoogleCollection")
        {
            Service.WindowManager.OpenWindow<MainWindow>().DisableWindowSounds = true;
        }
    }

    private void AddonObserver_AddonClose(string addonName)
    {
        if (Config.OpenWithMogpendium && addonName == "MoogleCollection")
        {
            Service.WindowManager.CloseWindow<MainWindow>();
        }
    }

    internal static GameFontHandle GetTripleTriadNumberFont(float size)
    {
        if (TripleTriadNumberFont == null || TripleTriadNumberFontSize != size)
        {
            TripleTriadNumberFont?.Dispose();
            TripleTriadNumberFont = Service.PluginInterface.UiBuilder.GetGameFontHandle(new GameFontStyle(GameFontFamily.MiedingerMid, size));
            TripleTriadNumberFontSize = size;
        }

        return TripleTriadNumberFont;
    }

    void IDisposable.Dispose()
    {
        Service.CommandManager.RemoveHandler("/mogmog");

        Service.PluginInterface.LanguageChanged -= PluginInterface_LanguageChanged;
        Service.PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        Service.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        Service.AddonObserver.AddonOpen -= AddonObserver_AddonOpen;
        Service.AddonObserver.AddonClose -= AddonObserver_AddonClose;

        TripleTriadNumberFont?.Dispose();

        Config?.Save();

        Service.Dispose();
    }
}
