using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MogMogCheck.Config;

namespace MogMogCheck;

public sealed class Plugin : IDalamudPlugin
{
    private readonly IHost _host;

    public Plugin(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog, IFramework framework)
    {
        _host = new HostBuilder()
            .UseContentRoot(pluginInterface.AssemblyLocation.Directory!.FullName)
            .ConfigureServices(services =>
            {
                services.AddDalamud(pluginInterface);
                services.AddConfig(PluginConfig.Load(pluginInterface, pluginLog));
                services.AddHaselCommon();
                services.AddMogMogCheck();
            })
            .Build();

        framework.RunOnFrameworkThread(_host.Start);
    }

    void IDisposable.Dispose()
    {
        _host.StopAsync().GetAwaiter().GetResult();
        _host.Dispose();
    }
}
