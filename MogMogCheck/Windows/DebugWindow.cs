using System.Linq;
using System.Reflection;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Utility;
using HaselCommon.Sheets;
using ImGuiNET;

namespace MogMogCheck.Windows;

public unsafe class DebugWindow : Window
{
    private readonly string[] ColumnNames = Array.Empty<string>();
    private int _shopId = 1769929;

    public DebugWindow() : base("MogMogCheck Debug")
    {
        Namespace = "MogMogCheckDebug";

        ColumnNames = typeof(ExtendedSpecialShop.SpecialShopItem).GetFields(BindingFlags.Instance | BindingFlags.Public).Select(x => x.Name).ToArray();
    }

    public override void Draw()
    {
        ImGui.InputInt("Shop Id", ref _shopId);

        var row = GetRow<ExtendedSpecialShop>((uint)_shopId)!;
        ImGui.TextUnformatted(row.Name.ToDalamudString().ToString());

        using var table = ImRaii.Table("##MogMogSheet", 27);

        for (var i = 0; i < ColumnNames.Length; i++)
        {
            ImGui.TableSetupColumn(ColumnNames[i]);
        }

        ImGui.TableHeadersRow();

        for (var i = 0; i < row.Items.Length; i++)
        {
            var item = row.Items[i];
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveCount1}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveCount2}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveCount1}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveCount2}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveCount3}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveItemId1}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveItemId2}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveSpecialShopItemCategory1}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ReceiveSpecialShopItemCategory2}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveItemId1}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveItemId2}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.GiveItemId3}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.UnlockQuest}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk38}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk3C}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk40}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk44}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk48}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk4C}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk50}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk52}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk54}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.PatchNumber}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk58}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk59}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5A}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5B}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5C}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5D}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5E}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk5F}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.SortKey}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk61}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk62}");
        }
    }
}
