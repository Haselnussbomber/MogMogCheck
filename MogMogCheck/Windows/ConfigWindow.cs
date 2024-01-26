using System.Numerics;
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

        Size = new Vector2(400, 120);
        SizeCondition = ImGuiCond.Appearing;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(400, 100),
            MaximumSize = new Vector2(4069),
        };
        Flags |= ImGuiWindowFlags.AlwaysAutoResize;
    }

    public override void OnClose()
    {
        Service.WindowManager.CloseWindow<ConfigWindow>();
    }

    public override void Draw()
    {
        // OpenWithMogpendium
        if (ImGui.Checkbox($"{t("Config.OpenWithMogpendium.Label")}##OpenWithMogpendium", ref Plugin.Config.OpenWithMogpendium))
        {
            Plugin.Config.Save();
        }

        // CheckboxMode
        {
            if (ImGui.Checkbox(t("Config.CheckboxMode"), ref Plugin.Config.CheckboxMode))
            {
                if (Plugin.Config.CheckboxMode)
                {
                    foreach (var (itemId, amount) in Plugin.Config.TrackedItems)
                    {
                        if (amount > 1)
                            Plugin.Config.TrackedItems[itemId] = 1;
                    }
                }

                Plugin.Config.Save();
            }

            ImGuiUtils.PushCursorY(-3);
            using var descriptionIndent = ImGuiUtils.ConfigIndent();
            ImGuiHelpers.SafeTextColoredWrapped(Colors.Grey, t("Config.CheckboxMode.Tooltip"));
            ImGuiUtils.PushCursorY(3);
        }
    }
}
