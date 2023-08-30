using Dalamud.Game.ClientState;
using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon;
using HaselCommon.Services;

namespace MogMogCheck;

public class Service
{
    public static TranslationManager TranslationManager => HaselCommonBase.TranslationManager;
    public static StringManager StringManager => HaselCommonBase.StringManager;
    public static TextureManager TextureManager => HaselCommonBase.TextureManager;
    public static WindowManager WindowManager => HaselCommonBase.WindowManager;

    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static CommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static ClientState ClientState { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
}
