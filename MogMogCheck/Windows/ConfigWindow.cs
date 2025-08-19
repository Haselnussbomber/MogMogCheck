using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using HaselCommon.Extensions;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Services;
using HaselCommon.Windows;
using MogMogCheck.Config;
using MogMogCheck.Tables;

namespace MogMogCheck.Windows;

[RegisterSingleton, AutoConstruct]
public unsafe partial class ConfigWindow : SimpleWindow
{
    private readonly IServiceProvider _serviceProvider;
    private readonly PluginConfig _pluginConfig;
    private readonly TextService _textService;

    [AutoPostConstruct]
    private void Initialize()
    {
        AllowClickthrough = false;
        AllowPinning = false;

        Flags |= ImGuiWindowFlags.AlwaysAutoResize;

        Size = new Vector2(380, -1);
        SizeCondition = ImGuiCond.Appearing;
    }

    public override void Draw()
    {
        // OpenWithMogpendium
        if (ImGui.Checkbox(_textService.Translate("Config.OpenWithMogpendium"), ref _pluginConfig.OpenWithMogpendium))
        {
            _pluginConfig.Save();
        }

        // OpenWithShop
        if (ImGui.Checkbox(_textService.Translate("Config.OpenWithShop"), ref _pluginConfig.OpenWithShop))
        {
            _pluginConfig.Save();
        }

        // CheckboxMode
        {
            if (ImGui.Checkbox(_textService.Translate("Config.CheckboxMode"), ref _pluginConfig.CheckboxMode))
            {
                if (_pluginConfig.CheckboxMode)
                {
                    foreach (var (itemId, amount) in _pluginConfig.TrackedItems)
                    {
                        if (amount > 1)
                            _pluginConfig.TrackedItems[itemId] = 1;
                    }
                }

                _pluginConfig.Save();

                if (_serviceProvider.TryGetService<ShopItemTable>(out var shopItemTable))
                    shopItemTable.SetReloadPending();
            }

            if (_pluginConfig.TrackedItems.Any(kv => kv.Value > 1))
            {
                ImGuiUtils.PushCursorY(-3);
                using var descriptionIndent = ImGuiUtils.ConfigIndent();
                ImGuiHelpers.SafeTextColoredWrapped(Color.Grey, _textService.Translate("Config.CheckboxMode.ResetInfo"));
            }

            using var indent = ImGuiUtils.ConfigIndent();
            using var disable = ImRaii.Disabled(!_pluginConfig.CheckboxMode);
            
            if (ImGui.Checkbox(_textService.Translate("Config.CheckboxMode.AutoUntrack"), ref _pluginConfig.AutoUntrack))
            {
                _pluginConfig.Save();
            }
        }

        // GrayOutCollectedItems
        if (ImGui.Checkbox(_textService.Translate("Config.GrayOutCollectedItems"), ref _pluginConfig.GrayOutCollectedItems))
        {
            _pluginConfig.Save();
        }

        /*
        // HidePreviousSeasons
        // Not implemented anymore with the rework. Might have to add this back at some time.
        {
            ImGuiUtils.PushCursorY(3);
            if (ImGui.Checkbox(_textService.Translate("Config.HidePreviousSeasons"), ref _pluginConfig.HidePreviousSeasons))
            {
                _pluginConfig.Save();

                if (Service.TryGet<SpecialShopService>(out var specialShopService))
                    specialShopService.SetIsDirty();
            }
        }
        */

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var cursorPos = ImGui.GetCursorPos();
        var contentAvail = ImGui.GetContentRegionAvail();

        ImGuiUtils.DrawLink("GitHub", _textService.Translate("ConfigWindow.GitHubLink.Tooltip"), "https://github.com/Haselnussbomber/MogMogCheck");
        ImGui.SameLine();
        ImGui.Text("•");
        ImGui.SameLine();
        ImGuiUtils.DrawLink("Ko-fi", _textService.Translate("ConfigWindow.KoFiLink.Tooltip"), "https://ko-fi.com/haselnussbomber");
        ImGui.SameLine();
        ImGui.Text("•");
        ImGui.SameLine();
        ImGui.Text(_textService.Translate("ConfigWindow.Licenses"));
        if (ImGui.IsItemHovered())
        {
            ImGui.SetMouseCursor(ImGuiMouseCursor.Hand);
            if (ImGui.IsMouseReleased(ImGuiMouseButton.Left) && _serviceProvider.TryGetService<LicensesWindow>(out var licensesWindow))
            {
                Task.Run(licensesWindow.Toggle);
            }
        }

        var version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version != null)
        {
            var versionString = "v" + version.ToString(3);
            ImGui.SetCursorPos(cursorPos + contentAvail - ImGui.CalcTextSize(versionString));
            ImGuiUtils.TextUnformattedDisabled(versionString);
        }
    }
}
