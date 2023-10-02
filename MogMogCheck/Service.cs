using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using HaselCommon;
using HaselCommon.Services;

namespace MogMogCheck;

public class Service
{
    public static TranslationManager TranslationManager { get; private set; }  = null!;
    public static StringManager StringManager { get; private set; } = null!;
    public static TextureManager TextureManager { get; private set; } = null!;
    public static WindowManager WindowManager { get; private set; } = null!;

    [PluginService] public static DalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] public static IClientState ClientState { get; private set; } = null!;
    [PluginService] public static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] public static IDataManager DataManager { get; private set; } = null!;
    [PluginService] public static IPluginLog PluginLog { get; private set; } = null!;
    [PluginService] public static ITextureProvider TextureProvider { get; private set; } = null!;

    internal static void Initialize(DalamudPluginInterface pluginInterface)
    {
        PluginInterface = pluginInterface;
        pluginInterface.Create<Service>();

        HaselCommonBase.Initialize(pluginInterface);
        TranslationManager = HaselCommonBase.TranslationManager;
        StringManager = HaselCommonBase.StringManager;
        TextureManager = HaselCommonBase.TextureManager;
        WindowManager = HaselCommonBase.WindowManager;
    }

    internal static void Dispose()
    {
        HaselCommonBase.Dispose();
        TranslationManager = null!;
        StringManager = null!;
        TextureManager = null!;
        WindowManager = null!;
    }
}
