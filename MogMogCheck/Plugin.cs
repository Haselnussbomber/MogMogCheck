using Dalamud.Plugin;
using HaselCommon;
using Microsoft.Extensions.DependencyInjection;
using MogMogCheck.Config;
using MogMogCheck.Services;

namespace MogMogCheck;

public sealed class Plugin : IDalamudPlugin
{
    public Plugin(IDalamudPluginInterface pluginInterface)
    {
        Service.Collection
            .AddDalamud(pluginInterface)
            .AddSingleton(PluginConfig.Load)
            .AddHaselCommon()
            .AddMogMogCheck();

        Service.Initialize(() =>
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
