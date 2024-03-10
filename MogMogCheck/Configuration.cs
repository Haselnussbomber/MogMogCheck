using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dalamud.Configuration;
using Dalamud.Interface.Internal.Notifications;
using Dalamud.Utility;

namespace MogMogCheck;

public partial class Configuration : IPluginConfiguration
{
    [JsonIgnore]
    public const int CURRENT_CONFIG_VERSION = 2;

    public int Version { get; set; } = CURRENT_CONFIG_VERSION;
}

public partial class Configuration
{
    public Dictionary<uint, uint> TrackedItems = [];
    public bool CheckboxMode = true;
    public bool OpenWithMogpendium = true;
    public bool HidePreviousSeasons = true;
}

// I really wish I could move this to HaselCommon, but I haven't found a way yet.
public partial class Configuration : IDisposable
{
    public static JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    [JsonIgnore]
    public int LastSavedConfigHash;

    public static Configuration Load()
    {
        try
        {
            var configPath = Service.PluginInterface.ConfigFile.FullName;
            if (!File.Exists(configPath))
                return new();

            var jsonData = File.ReadAllText(configPath);
            if (string.IsNullOrEmpty(jsonData))
                return new();

            var config = JsonNode.Parse(jsonData);
            if (config is not JsonObject configObject)
                return new();

            var version = (int?)configObject[nameof(Version)] ?? 0;
            if (version == 0)
                return new();

            if (version < CURRENT_CONFIG_VERSION)
            {
                try
                {
                    var configBackupPath = configPath + ".bak";
                    var jsonBackupData = File.Exists(configBackupPath) ? File.ReadAllText(configBackupPath) : null;
                    if (string.IsNullOrEmpty(jsonBackupData) || !string.Equals(jsonData, jsonBackupData))
                    {
                        File.Copy(configPath, configBackupPath, true);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("Could not back up config before migration", ex);
                }

                Service.PluginLog.Information("Starting config migration: {currentVersion} -> {targetVersion}", version, CURRENT_CONFIG_VERSION);

                Migrate(version, configObject);

                config[nameof(Version)] = CURRENT_CONFIG_VERSION;

                Service.PluginLog.Information("Config migration completed.");
            }

            var deserializedConfig = configObject.Deserialize<Configuration>(DefaultJsonSerializerOptions);
            if (deserializedConfig == null)
                return new();

            deserializedConfig.Save();

            return deserializedConfig;
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Could not load the configuration file. Creating a new one.");

            if (!Service.TranslationManager.TryGetTranslation("Plugin.DisplayName", out var pluginName))
                pluginName = Service.PluginInterface.InternalName;

            Service.PluginInterface.UiBuilder.AddNotification(
                t("Notification.CouldNotLoadConfig"),
                pluginName,
                NotificationType.Error,
                5000
            );

            return new();
        }
    }

    public static void Migrate(int version, JsonObject config)
    {
        if (version < 2)
        {
            config.Remove("TrackedItems");
        }
    }

    public void Save()
    {
        try
        {
            var serialized = JsonSerializer.Serialize(this, DefaultJsonSerializerOptions);
            var hash = serialized.GetHashCode();

            if (LastSavedConfigHash != hash)
            {
                Util.WriteAllTextSafe(Service.PluginInterface.ConfigFile.FullName, serialized);
                LastSavedConfigHash = hash;
                Service.PluginLog.Information("Configuration saved.");
            }
        }
        catch (Exception e)
        {
            Service.PluginLog.Error(e, "Error saving config");
        }
    }

    void IDisposable.Dispose()
    {
        Save();
    }
}
