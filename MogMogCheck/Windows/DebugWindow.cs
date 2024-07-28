#if DEBUG
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using HaselCommon.Services;
using HaselCommon.Windowing;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets2;
using MogMogCheck.Services;

namespace MogMogCheck.Windows;

public unsafe class DebugWindow(
    WindowManager WindowManager,
    IClientState ClientState,
    ExcelService ExcelService,
    TextureService TextureService,
    SpecialShopService SpecialShopService)
    : SimpleWindow(WindowManager, "MogMogCheck Debug")
{
    private int ShopId = 1769929; //1770710;

    public override void Draw()
    {
        ImGui.TextUnformatted($"IsLoggedIn: {ClientState.IsLoggedIn}");
        ImGui.TextUnformatted($"CurrentSeason: {SpecialShopService.CurrentSeason}");
        ImGui.TextUnformatted($"CurrentShop: {SpecialShopService.CurrentShop?.RowId}");
        ImGui.TextUnformatted($"RewardsItems: {SpecialShopService.RewardsItems.Length}");
        ImGui.TextUnformatted($"TomestoneItem: {SpecialShopService.TomestoneItems.Count}");

        ImGui.InputInt("Shop Id", ref ShopId);

        var row = ExcelService.GetRow<SpecialShop>((uint)ShopId)!;
        ImGui.TextUnformatted(row.Name.ToDalamudString().ToString());

        for (var i = 0; i < row.Item.Length; i++)
        {
            var item = row.Item[i];
            if (item.Item[0].Row == 0)
                continue;

            var itemRow = ExcelService.GetRow<Item>(item.Item[0].Row)!;

            using var node = ImRaii.TreeNode($"{itemRow.Name}##ShopItem{i}", ImGuiTreeNodeFlags.SpanAvailWidth);
            if (!node)
                continue;

            TextureService.DrawIcon(itemRow.Icon, 48);

            using var table = ImRaii.Table($"ShopItem{i}Table", 2);
            if (!table)
                continue;

            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 210);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ReceiveCount[0]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveCount[0]}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ReceiveCount[1]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveCount[1]}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("CurrencyCost[0]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.CurrencyCost[0]}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("CurrencyCost[1]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.CurrencyCost[1]}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("CurrencyCost[2]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.CurrencyCost[2]}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Item[0]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Item[0].Row}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Item[1]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Item[1].Row}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Category[0]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Category[0].Row}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Category[1]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Category[1].Row}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ItemCost[0]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ItemCost[0]}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ItemCost[1]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ItemCost[1]}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ItemCost[2]");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ItemCost[2]}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("UnlockQuest");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Quest.Row}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("PatchNumber");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.PatchNumber}");

            for (var j = 0; j < item.Unknown0.Length; j++)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"Unknown0[{j}]");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{item.Unknown0[j]}");
            }

            for (var j = 0; j < item.Unknown1.Length; j++)
            {
                ImGui.TableNextRow();
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"Unknown1[{j}]");
                ImGui.TableNextColumn();
                ImGui.TextUnformatted($"{item.Unknown1[j]}");
            }

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unknown2");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unknown2}");
        }
    }
}
#endif
