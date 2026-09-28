using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.System.Framework;
using HaselCommon.Windows;

namespace MogMogCheck.Windows;

[RegisterSingleton, AutoConstruct]
public unsafe partial class DebugWindow : SimpleDebugWindow
{
    [AutoPostConstruct]
    private void Initialize()
    {
        RegisterTab(new EventInfoTab());
    }

    public class EventInfoTab : ISimpleDebugTab
    {
        public string Name => "EventInfo";

        public void Draw()
        {
            ref var eventInfo = ref CSBonusManager.Instance()->EventInfo;
            ImGui.Text($"Season: {eventInfo.Season}");
            ImGui.Text($"BaseTime: {eventInfo.BaseTime} (Week: {(Framework.GetServerTime() - eventInfo.BaseTime) / 604800 + 1})");
            ImGui.Text($"SeasonTarget: {eventInfo.SeasonTarget}");
            ImGui.Text($"IsOpenShop: {eventInfo.IsOpenShop}");
            ImGui.Text($"IsOpenMission: {eventInfo.IsOpenMission}");
        }
    }
}
