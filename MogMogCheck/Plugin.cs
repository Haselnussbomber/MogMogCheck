using System.Threading.Tasks;
using Dalamud.Interface.GameFonts;
using Dalamud.Plugin;
using HaselCommon.Extensions;
using MogMogCheck.Windows;

namespace MogMogCheck;

public sealed partial class Plugin : IDalamudPlugin
{
    public static Configuration Config { get; private set; } = null!;
    private static GameFontHandle? TripleTriadNumberFont = null!;
    private static float TripleTriadNumberFontSize = 0;

    public Plugin(DalamudPluginInterface pluginInterface)
    {
        Service.Initialize(pluginInterface);
        Task.Run(HaselCommon.Interop.Resolver.GetInstance.Resolve)
            .ContinueOnFrameworkThreadWith(Setup);
    }

    public void Setup()
    {
        Service.TranslationManager.Initialize();

        Config = Configuration.Load();

        Service.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;

        Service.CommandManager.AddHandler("/mogmog", new(OnCommand) { HelpMessage = t("CommandHandlerHelpMessage") });
    }

    private void OnCommand(string command, string arguments)
    {
        Service.WindowManager.ToggleWindow<MainWindow>();
    }

    void IDisposable.Dispose()
    {
        Service.CommandManager.RemoveHandler("/mogmog");

        Service.PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;

        TripleTriadNumberFont?.Dispose();

        Config?.Save();

        Service.Dispose();
    }

    private void OpenMainUi()
    {
        Service.WindowManager.ToggleWindow<MainWindow>();
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
}
