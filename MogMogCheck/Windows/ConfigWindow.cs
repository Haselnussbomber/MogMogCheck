using System.Linq;
using System.Numerics;
using Dalamud.Plugin;
using HaselCommon.Services;
using HaselCommon.Utils;
using HaselCommon.Windowing;
using ImGuiNET;
using MogMogCheck.Config;
using MogMogCheck.Services;

namespace MogMogCheck.Windows;

public unsafe class ConfigWindow : SimpleWindow
{
    private readonly IDalamudPluginInterface PluginInterface;
    private readonly PluginConfig PluginConfig;
    private readonly TextService TextService;
    private readonly SpecialShopService SpecialShopService;

    public ConfigWindow(
        WindowManager windowManager,
        IDalamudPluginInterface pluginInterface,
        PluginConfig pluginConfig,
        TextService textService,
        SpecialShopService specialShopService) : base(windowManager, "MogMogCheck Configuration")
    {
        PluginInterface = pluginInterface;
        PluginConfig = pluginConfig;
        TextService = textService;
        SpecialShopService = specialShopService;

        AllowClickthrough = false;
        AllowPinning = false;

        Flags |= ImGuiWindowFlags.AlwaysAutoResize;

        Size = new Vector2(380, -1);
        SizeCondition = ImGuiCond.Appearing;

        PluginInterface.UiBuilder.OpenConfigUi += Toggle;
    }

    public new void Dispose()
    {
        PluginInterface.UiBuilder.OpenConfigUi -= Toggle;
        base.Dispose();
    }

    public override void Draw()
    {
        // OpenWithMogpendium
        if (ImGui.Checkbox($"{TextService.Translate("Config.OpenWithMogpendium.Label")}##OpenWithMogpendium", ref PluginConfig.OpenWithMogpendium))
        {
            PluginConfig.Save();
        }

        // CheckboxMode
        {
            if (ImGui.Checkbox(TextService.Translate("Config.CheckboxMode"), ref PluginConfig.CheckboxMode))
            {
                if (PluginConfig.CheckboxMode)
                {
                    foreach (var (itemId, amount) in PluginConfig.TrackedItems)
                    {
                        if (amount > 1)
                            PluginConfig.TrackedItems[itemId] = 1;
                    }
                }

                PluginConfig.Save();
            }

            if (PluginConfig.TrackedItems.Any(kv => kv.Value > 1))
            {
                ImGuiUtils.PushCursorY(-3);
                using var descriptionIndent = ImGuiUtils.ConfigIndent();
                TextService.DrawWrapped(Colors.Grey, "Config.CheckboxMode.Tooltip");
                ImGuiUtils.PushCursorY(3);
            }
        }

        // HidePreviousSeasons
        {
            if (ImGui.Checkbox(TextService.Translate("Config.HidePreviousSeasons"), ref PluginConfig.HidePreviousSeasons))
            {
                PluginConfig.Save();
                SpecialShopService.IsDirty = true;
            }
        }
    }
}
