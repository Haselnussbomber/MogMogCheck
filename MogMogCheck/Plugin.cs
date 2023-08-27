using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;
using Dalamud.Plugin;
using HaselCommon;
using HaselCommon.Extensions;
using MogMogCheck.Windows;

namespace MogMogCheck;

public sealed partial class Plugin : IDalamudPlugin
{
    public string Name => "MogMogCheck";

    public static Configuration Config { get; private set; } = null!;

    public unsafe Plugin(DalamudPluginInterface pluginInterface)
    {
        pluginInterface.Create<Service>();
        HaselCommonBase.Initialize(pluginInterface);

        Config = Configuration.Load();
        Config.TrackedItems.RemoveAll((int itemId, bool tracked) => !tracked); // clear old untracked items

        using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("MogMogCheck.Translations.json")
            ?? throw new Exception($"Could not find translations resource \"MogMogCheck.Translations.json\".");

        var translations = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(stream) ?? new();

        Service.TranslationManager.Initialize(translations, Config);

        Service.PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        //Service.PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;

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
        //Service.PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;

        Service.TranslationManager.OnLanguageChange -= Config.Save;

        Config?.Save();
        HaselCommonBase.Dispose();
    }

    private void OpenMainUi()
    {
        Service.WindowManager.ToggleWindow<MainWindow>();
    }

    private void OpenConfigUi()
    {
        Service.WindowManager.ToggleWindow<ConfigWindow>();
    }
}
