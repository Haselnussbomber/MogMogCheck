#if DEBUG
using System.Linq;
using System.Reflection;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using HaselCommon.Sheets;
using ImGuiNET;
using MogMogCheck.Structs;

namespace MogMogCheck.Windows;

public unsafe class DebugWindow : Window
{
    private readonly string[] ColumnNames = Array.Empty<string>();
    private int _shopId = 1770710; //1769929;

    public DebugWindow() : base("MogMogCheck Debug")
    {
        Namespace = "MogMogCheckDebug";

        ColumnNames = typeof(ExtendedSpecialShop.SpecialShopItem).GetFields(BindingFlags.Instance | BindingFlags.Public).Select(x => x.Name).ToArray();
    }

    public override void Draw()
    {
        if (ImGui.Button("Print CSBonus debug info"))
        {
            CSBonusManager.Instance()->PrintDebugInfo();
        }
        ImGui.SameLine();
        if (ImGui.Button("RequestData"))
        {
            CSBonusManager.Instance()->RequestData();
        }

        ImGui.InputInt("Shop Id", ref _shopId);

        var row = GetRow<ExtendedSpecialShop>((uint)_shopId)!;
        ImGui.TextUnformatted(row.Name.ToDalamudString().ToString());

        for (var i = 0; i < row.Items.Length; i++)
        {
            var item = row.Items[i];
            if (item.ReceiveItemId1 == 0)
                continue;

            var receiveItemId1 = GetRow<ExtendedItem>((uint)item.ReceiveItemId1)!;

            using var node = ImRaii.TreeNode($"{receiveItemId1.Name}##ShopItem{i}", ImGuiTreeNodeFlags.SpanAvailWidth);
            if (!node)
                continue;

            using var table = ImRaii.Table($"ShopItem{i}Table", 2);
            if (!table)
                continue;

            ImGui.TableSetupColumn("Label", ImGuiTableColumnFlags.WidthFixed, 210);

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ReceiveCount1");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveCount1}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ReceiveCount2");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveCount2}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("GiveCount1");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveCount1}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("GiveCount2");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveCount2}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("GiveCount3");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveCount3}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ReceiveItemId1");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveItemId1}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ReceiveItemId2");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveItemId2}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ReceiveSpecialShopItemCategory1");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveSpecialShopItemCategory1}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("ReceiveSpecialShopItemCategory2");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveSpecialShopItemCategory2}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("GiveItemId1");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveItemId1}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("GiveItemId2");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveItemId2}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("GiveItemId3");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveItemId3}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("UnlockQuest");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.UnlockQuest}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk38");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk38}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk3C");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk3C}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk40");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk40}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk44");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk44}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk48");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk48}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk4C");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk4C}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk50");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk50}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk52");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk52}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk54");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk54}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("PatchNumber");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.PatchNumber}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk58");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk58}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk59");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk59}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk5A");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5A}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk5B");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5B}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk5C");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5C}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk5D");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5D}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk5E");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5E}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk5F");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5F}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("SortKey");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.SortKey}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk61");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk61}");

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("Unk62");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk62}");
        }
    }
}
#endif
