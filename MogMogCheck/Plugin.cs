using System.IO;
using Dalamud.Game;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon;
using HaselCommon.Logger;
using InteropGenerator.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MogMogCheck.Caches;
using MogMogCheck.Config;
using MogMogCheck.Services;
using MogMogCheck.Windows;

namespace MogMogCheck;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin(
        IDalamudPluginInterface pluginInterface,
        IFramework framework,
        IPluginLog pluginLog,
        ISigScanner sigScanner,
        IDataManager dataManager)
    {
        Service
            // Dalamud & HaselCommon
            .Initialize(pluginInterface)

            // Logging
            .AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Trace);
                builder.AddProvider(new DalamudLoggerProvider(pluginLog));
            })

            // Config
            .AddSingleton(PluginConfig.Load(pluginInterface, pluginLog))

            // Caches
            .AddSingleton<ItemQuantityCache>()

            // Services
            .AddSingleton<TripleTriadNumberFontManager>()
            .AddSingleton<SpecialShopService>()

            // Windows
            .AddSingleton<MainWindow>()
#if DEBUG
            .AddSingleton<DebugWindow>()
#endif
            .AddSingleton<ConfigWindow>();

        Service.BuildProvider();

        // ---

        FFXIVClientStructs.Interop.Generated.Addresses.Register();
        Resolver.GetInstance.Setup(
            sigScanner.SearchBase,
            dataManager.GameData.Repositories["ffxiv"].Version,
            new FileInfo(Path.Join(pluginInterface.ConfigDirectory.FullName, "SigCache.json")));
        Resolver.GetInstance.Resolve();

        // ---

        framework.RunOnFrameworkThread(() =>
        {
            Service.Get<MainWindow>();
#if DEBUG
            //Service.Get<DebugWindow>().Open();
#endif
        });
    }

    void IDisposable.Dispose()
    {
        Service.Dispose();
    }
}
