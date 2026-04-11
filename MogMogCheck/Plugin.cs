using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MogMogCheck.Config;

namespace MogMogCheck;

public sealed class Plugin(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog) : IAsyncDalamudPlugin
{
    private readonly IHost _host = new HostBuilder()
        .UseContentRoot(pluginInterface.AssemblyLocation.Directory!.FullName)
        .ConfigureServices(services =>
        {
            services.AddDalamud(pluginInterface);
            services.AddConfig(PluginConfig.Load(pluginInterface, pluginLog));
            services.AddHaselCommon();
            services.AddMogMogCheck();
        })
        .Build();

    public ValueTask LoadAsync(CancellationToken cancellationToken)
    {
        return new ValueTask(_host.StartAsync(cancellationToken));
    }

    public async ValueTask DisposeAsync()
    {
        await _host.StopAsync();
        _host.Dispose();
    }
}
