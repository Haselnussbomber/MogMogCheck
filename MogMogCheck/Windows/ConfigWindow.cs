using System.Numerics;
using Dalamud.Interface;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using HaselCommon.Enums;
using HaselCommon.Extensions;
using HaselCommon.Services;
using HaselCommon.Utils;
using ImGuiNET;

namespace MogMogCheck.Windows;

public unsafe class ConfigWindow : Window
{
    public ConfigWindow() : base("MogMogCheck Configuration", ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoCollapse)
    {
        Namespace = "MogMogCheckConfig";

        Size = new Vector2(300, 200);
        SizeCondition = ImGuiCond.Appearing;

        AllowClickthrough = false;
        AllowPinning = false;
    }

    public override void OnClose()
    {
        Service.WindowManager.CloseWindow<ConfigWindow>();
    }

    public override void Draw()
    {
        // Language
        {
            ImGui.Spacing();
            ImGuiUtils.PushCursorY(ImGui.GetStyle().FramePadding.Y);
            ImGui.TextUnformatted("Language:");
            ImGui.SameLine();
            ImGuiUtils.PushCursorY(-ImGui.GetStyle().FramePadding.Y / 2f);

            static string GetLabel(string type, string code)
            {
                return TranslationManager.AllowedLanguages.ContainsKey(code)
                    ? $"Override: {type} ({code})"
                    : $"Override: {type} ({code} is not supported, using fallback {TranslationManager.DefaultLanguage})";
            }

            var previewValue = Service.TranslationManager.Override switch
            {
                PluginLanguageOverride.Dalamud => GetLabel("Dalamud", Service.PluginInterface.UiLanguage),
                PluginLanguageOverride.Client => GetLabel("Client", Service.ClientState.ClientLanguage.ToCode()),
                _ => TranslationManager.AllowedLanguages[Plugin.Config.PluginLanguage]
            };

            using (var combo = ImRaii.Combo("##Language", previewValue))
            {
                if (combo)
                {
                    if (ImGui.Selectable($"{GetLabel("Dalamud", Service.PluginInterface.UiLanguage)}##Language_Dalamud", Service.TranslationManager.Override == PluginLanguageOverride.Dalamud))
                    {
                        Service.TranslationManager.Override = PluginLanguageOverride.Dalamud;
                    }

                    if (ImGui.Selectable($"{GetLabel("Client", Service.ClientState.ClientLanguage.ToCode())}##Language_Client", Service.TranslationManager.Override == PluginLanguageOverride.Client))
                    {
                        Service.TranslationManager.Override = PluginLanguageOverride.Client;
                    }

                    ImGui.Separator();

                    foreach (var (code, name) in TranslationManager.AllowedLanguages)
                    {
                        if (ImGui.Selectable($"{name}##Language_{code}", Service.TranslationManager.Override == PluginLanguageOverride.None && Service.TranslationManager.Language == code))
                        {
                            Service.TranslationManager.SetLanguage(PluginLanguageOverride.None, code);
                        }
                    }
                }
            }

            //ImGuiComponents.HelpMarker(t("Config.LanguageNote"));
            ImGui.SameLine();
            ImGuiUtils.PushCursorY(-ImGui.GetStyle().FramePadding.Y / 2f);
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.TextDisabled(FontAwesomeIcon.InfoCircle.ToIconString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered())
            {
                ImGui.BeginTooltip();
                ImGui.PushTextWrapPos(ImGui.GetFontSize() * 35f);
                ImGui.TextUnformatted(t("Config.LanguageNote"));
                ImGui.PopTextWrapPos();
                ImGui.EndTooltip();
            }
        }

        ImGui.Spacing();
    }
}
