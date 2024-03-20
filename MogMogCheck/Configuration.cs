using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using HaselCommon;
using HaselCommon.Interfaces;

namespace MogMogCheck;

public partial class Configuration : IConfiguration
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

public partial class Configuration
{
    [JsonIgnore]
    public int LastSavedConfigHash { get; set; }

    public void Save()
        => ConfigurationManager.Save(this);

    public string Serialize()
        => JsonSerializer.Serialize(this, ConfigurationManager.DefaultSerializerOptions);

    public static Configuration Load()
        => ConfigurationManager.Load(CURRENT_CONFIG_VERSION, Deserialize, Migrate);

    public static Configuration? Deserialize(ref JsonObject config)
        => config.Deserialize<Configuration>(ConfigurationManager.DefaultSerializerOptions);

    public static bool Migrate(int version, ref JsonObject config)
    {
        if (version < 2)
        {
            config.Remove("TrackedItems");
        }

        return true;
    }
}
