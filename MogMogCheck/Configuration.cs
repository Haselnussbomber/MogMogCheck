using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Dalamud.Configuration;
using Dalamud.Interface.Internal.Notifications;
using HaselCommon.Enums;
using HaselCommon.Interfaces;

namespace MogMogCheck;

public partial class Configuration : IPluginConfiguration
{
    [JsonIgnore]
    public const int CURRENT_CONFIG_VERSION = 2;

    public int Version { get; set; } = CURRENT_CONFIG_VERSION;
}

public partial class Configuration : ITranslationConfig
{
    public string PluginLanguage { get; set; } = "en";
    public PluginLanguageOverride PluginLanguageOverride { get; set; } = PluginLanguageOverride.Dalamud;
}

public partial class Configuration
{
    public Dictionary<uint, uint> TrackedItems = [];
    public bool LimitToOne = true;
}

public partial class Configuration
{
    public static JsonSerializerOptions DefaultJsonSerializerOptions = new()
    {
        IncludeFields = true,
        WriteIndented = true
    };

    public static int LastSavedConfigHash { get; private set; } = 0;

    internal static Configuration Load()
    {
        var configPath = Service.PluginInterface.ConfigFile.FullName;
        var configBackupPath = configPath + ".bak";
        var jsonData = string.Empty;
        JsonNode? config = null;

        try
        {
            jsonData = File.Exists(configPath) ? File.ReadAllText(configPath) : null;

            if (string.IsNullOrEmpty(jsonData))
                return new();

            config = JsonNode.Parse(jsonData);
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

        if (config is not JsonObject configObject)
            return new();

        var version = (int?)configObject[nameof(Version)] ?? 0;
        if (version == 0)
            return new();

        var needsMigration = version < CURRENT_CONFIG_VERSION;
        if (needsMigration)
        {
            try
            {
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

            Service.PluginLog.Information($"Starting config migration: {version} -> {CURRENT_CONFIG_VERSION}");

            try
            {
                if (version < 2)
                {
                    configObject.Remove("TrackedItems");
                }

                configObject[nameof(Version)] = CURRENT_CONFIG_VERSION;
            }
            catch (Exception ex)
            {
                Service.PluginLog.Error(ex, "Could not migrate configuration");
                // continue, for now
            }

            Service.PluginLog.Information("Config migration completed.");
        }

        Configuration? deserializedConfig;

        try
        {
            deserializedConfig = configObject.Deserialize<Configuration>(DefaultJsonSerializerOptions);
        }
        catch (Exception ex)
        {
            Service.PluginLog.Error(ex, "Could not deserialize configuration, creating a new one");

            Service.PluginInterface.UiBuilder.AddNotification(
                t("Config.CouldNotLoadConfigNotification"),
                "MogMogCheck",
                NotificationType.Error,
                5000
            );

            return new();
        }

        if (deserializedConfig == null)
            return new();

        if (needsMigration)
        {
            deserializedConfig.Save();
        }
        else
        {
            try
            {
                var serialized = JsonSerializer.Serialize(deserializedConfig, DefaultJsonSerializerOptions);
                LastSavedConfigHash = serialized.GetHashCode();
            }
            catch (Exception e)
            {
                Service.PluginLog.Error(e, "Error generating config hash");
            }
        }

        return deserializedConfig;
    }

    internal void Save()
    {
        try
        {
            var configPath = Service.PluginInterface.ConfigFile.FullName;
            var serialized = JsonSerializer.Serialize(this, DefaultJsonSerializerOptions);
            var hash = serialized.GetHashCode();

            if (LastSavedConfigHash != hash)
            {
                File.WriteAllText(configPath, serialized);
                LastSavedConfigHash = hash;
                Service.PluginLog.Information("Configuration saved.");
            }
        }
        catch (Exception e)
        {
            Service.PluginLog.Error(e, "Error saving config");
        }
    }
}
