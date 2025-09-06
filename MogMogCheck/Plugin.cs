using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using Microsoft.Extensions.DependencyInjection;
using MogMogCheck.Config;
using MogMogCheck.Services;

namespace MogMogCheck;

public sealed class Plugin : IDalamudPlugin
{
    private readonly ServiceProvider _serviceProvider;

    public Plugin(IDalamudPluginInterface pluginInterface, IFramework framework)
    {
#if CUSTOM_CS
        pluginInterface.InitializeCustomClientStructs();
#endif

        _serviceProvider = new ServiceCollection()
            .AddDalamud(pluginInterface)
            .AddSingleton(PluginConfig.Load)
            .AddHaselCommon()
            .AddMogMogCheck()
            .BuildServiceProvider();

        framework.RunOnFrameworkThread(() =>
        {
            _serviceProvider.GetRequiredService<CommandManager>();
            _serviceProvider.GetRequiredService<AutoUntrackService>();
        });
    }

    void IDisposable.Dispose()
    {
        _serviceProvider.Dispose();
    }
}
