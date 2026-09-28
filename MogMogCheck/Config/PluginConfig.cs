using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dalamud.Configuration;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Microsoft.Extensions.DependencyInjection;

namespace MogMogCheck.Config;

[RegisterSingleton(Factory = nameof(ServiceFactory))]
public partial class PluginConfig : IPluginConfiguration
{
    [JsonIgnore]
    public const int CURRENT_CONFIG_VERSION = 2;

    [JsonIgnore]
    public int LastSavedConfigHash { get; set; }

    [JsonIgnore]
    public static JsonSerializerOptions? SerializerOptions { get; } = new()
    {
        IncludeFields = true,
        WriteIndented = true,
    };

    [JsonIgnore]
    private static IDalamudPluginInterface? PluginInterface;

    [JsonIgnore]
    private static IPluginLog? PluginLog;

    public static PluginConfig ServiceFactory(IServiceProvider serviceProvider)
    {
        PluginInterface = serviceProvider.GetRequiredService<IDalamudPluginInterface>();
        PluginLog = serviceProvider.GetRequiredService<IPluginLog>();

        var fileInfo = PluginInterface.ConfigFile;
        if (!fileInfo.Exists || fileInfo.Length < 2)
            return new();

        var json = File.ReadAllText(fileInfo.FullName);
        if (JsonNode.Parse(json) is not JsonObject node)
            return new();

        return JsonSerializer.Deserialize<PluginConfig>(node, SerializerOptions) ?? new();
    }

    public void Save()
    {
        try
        {
            var serialized = JsonSerializer.Serialize(this, SerializerOptions);
            var hash = StringComparer.Ordinal.GetHashCode(serialized);

            if (LastSavedConfigHash != hash)
            {
                FilesystemUtil.WriteAllTextSafe(PluginInterface!.ConfigFile.FullName, serialized);
                LastSavedConfigHash = hash;
                PluginLog?.Information("Configuration saved.");
            }
        }
        catch (Exception e)
        {
            PluginLog?.Error(e, "Error saving config");
        }
    }
}

public partial class PluginConfig
{
    public int Version { get; set; } = CURRENT_CONFIG_VERSION;
    public Dictionary<uint, uint> TrackedItems = [];
    public bool CheckboxMode = true;
    public bool AutoUntrack = true;
    public bool OpenWithMogpendium = true;
    public bool OpenWithShop = true;
    public bool GrayOutCollectedItems = false;
    public bool HidePreviousSeasons = true;
}
