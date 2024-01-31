using Dalamud.Interface.Utility;
using Dalamud.Interface.Windowing;
using HaselCommon.Utils;
using ImGuiNET;

namespace MogMogCheck.Windows;

public unsafe class ConfigWindow : Window
{
    public ConfigWindow() : base("MogMogCheck Configuration")
    {
        Namespace = "MogMogCheckConfig";
        AllowClickthrough = false;
        AllowPinning = false;
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void OnClose()
    {
        Service.WindowManager.CloseWindow<ConfigWindow>();
    }

    public override void Draw()
    {
        var config = Service.GetService<Configuration>();

        // OpenWithMogpendium
        if (ImGui.Checkbox($"{t("Config.OpenWithMogpendium.Label")}##OpenWithMogpendium", ref config.OpenWithMogpendium))
        {
            config.Save();
        }

        // CheckboxMode
        {
            if (ImGui.Checkbox(t("Config.CheckboxMode"), ref config.CheckboxMode))
            {
                if (config.CheckboxMode)
                {
                    foreach (var (itemId, amount) in config.TrackedItems)
                    {
                        if (amount > 1)
                            config.TrackedItems[itemId] = 1;
                    }
                }

                config.Save();
            }

            ImGuiUtils.PushCursorY(-3);
            using var descriptionIndent = ImGuiUtils.ConfigIndent();
            ImGuiHelpers.SafeTextColoredWrapped(Colors.Grey, t("Config.CheckboxMode.Tooltip"));
            ImGuiUtils.PushCursorY(3);
        }
    }
}
