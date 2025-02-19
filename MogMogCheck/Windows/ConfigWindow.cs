using System.Linq;
using System.Numerics;
using Dalamud.Interface.Utility;
using HaselCommon;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using MogMogCheck.Config;
using MogMogCheck.Services;
using MogMogCheck.Tables;

namespace MogMogCheck.Windows;

[RegisterSingleton, AutoConstruct]
public unsafe partial class ConfigWindow : SimpleWindow
{
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
        if (ImGui.Checkbox($"{_textService.Translate("Config.OpenWithMogpendium.Label")}##OpenWithMogpendium", ref _pluginConfig.OpenWithMogpendium))
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

                if (Service.TryGet<ShopItemTable>(out var shopItemTable))
                    shopItemTable.SetReloadPending();
            }

            if (_pluginConfig.TrackedItems.Any(kv => kv.Value > 1))
            {
                ImGuiUtils.PushCursorY(-3);
                using var descriptionIndent = ImGuiUtils.ConfigIndent();
                ImGuiHelpers.SafeTextColoredWrapped(Color.Grey, _textService.Translate("Config.CheckboxMode.Tooltip"));
                ImGuiUtils.PushCursorY(3);
            }
        }

        // HidePreviousSeasons
        {
            if (ImGui.Checkbox(_textService.Translate("Config.HidePreviousSeasons"), ref _pluginConfig.HidePreviousSeasons))
            {
                _pluginConfig.Save();

                if (Service.TryGet<SpecialShopService>(out var specialShopService))
                    specialShopService.SetIsDirty();
            }
        }
    }
}
