using System.Collections.Generic;
using System.IO;
using Dalamud.Configuration;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Logging;
using HaselCommon.Enums;
using HaselCommon.Interfaces;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MogMogCheck;

[Serializable]
public partial class Configuration : IPluginConfiguration, ITranslationConfig
{
    [JsonIgnore]
    public const int CURRENT_CONFIG_VERSION = 1;

    public int Version { get; set; } = CURRENT_CONFIG_VERSION;

    public string PluginLanguage { get; set; } = "en";
    public PluginLanguageOverride PluginLanguageOverride { get; set; } = PluginLanguageOverride.Client;

    public Dictionary<uint, bool> TrackedItems = new();
}

public partial class Configuration
{
    internal static Configuration Load()
    {
        var configPath = Service.PluginInterface.ConfigFile.FullName;
        JObject? config = null;

        try
        {
            var jsonData = File.Exists(configPath) ? File.ReadAllText(configPath) : null;
            if (string.IsNullOrEmpty(jsonData))
                return new();

            config = JObject.Parse(jsonData);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Could not load configuration, creating a new one");

            Service.PluginInterface.UiBuilder.AddNotification(
                t("Config.CouldNotLoadConfigNotification"),
                "MogMogCheck",
                NotificationType.Error,
                5000
            );
        }

        // possible migrations here

        return config?.ToObject<Configuration>() ?? new();
    }

    internal void Save()
    {
        Service.PluginLog.Information("Configuration saved.");
        Service.PluginInterface.SavePluginConfig(this);
    }
}
