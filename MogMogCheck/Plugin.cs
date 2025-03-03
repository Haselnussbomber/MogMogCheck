using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon;
using Microsoft.Extensions.DependencyInjection;
using MogMogCheck.Config;
using MogMogCheck.Services;

namespace MogMogCheck;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface, IFramework framework)
    {
        Service.Collection
            .AddDalamud(pluginInterface)
            .AddSingleton(PluginConfig.Load)
            .AddHaselCommon()
            .AddMogMogCheck();

        Service.BuildProvider();

        framework.RunOnFrameworkThread(() =>
        {
            Service.Get<CommandManager>();
            Service.Get<AutoUntrackService>();
        });
    }

    void IDisposable.Dispose()
    {
        Service.Dispose();
    }
}
