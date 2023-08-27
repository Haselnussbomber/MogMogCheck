using System.Linq;
using System.Reflection;
using Dalamud.Interface.Raii;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using MogMogCheck.Sheets;

namespace MogMogCheck.Windows;

public unsafe class DebugWindow : Window
{
    private readonly string[] ColumnNames = Array.Empty<string>();

    public DebugWindow() : base("MogMogCheck Debug")
    {
        Namespace = "MogMogCheckDebug";

        ColumnNames = typeof(SpecialShop.SpecialShopItem).GetFields(BindingFlags.Instance | BindingFlags.Public).Select(x => x.Name).ToArray();
    }

    public override void Draw()
    {
        ImGui.TextUnformatted("SpecialShop#1769929");

        using var table = ImRaii.Table("##MogMogSheet", 27);

        for (var i = 0; i < ColumnNames.Length; i++)
        {
            ImGui.TableSetupColumn(ColumnNames[i]);
        }

        ImGui.TableHeadersRow();

        var row = GetRow<SpecialShop>(1769929)!;
        for (var i = 0; i < row.Items.Length; i++)
        {
            var item = row.Items[i];
            ImGui.TableNextRow();

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.StackSize}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk8}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.RequiredCount}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk10}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk14}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.ItemId}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk1C}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk20}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk24}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.RequiredItem}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk2C}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk30}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.RequiredQuest}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk38}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk3C}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk40}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk42}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk44}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.PatchNumber}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk48}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk49}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk4A}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk4B}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk4C}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.SortKey}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk4E}");

            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{item.Unk4F}");
        }
    }
}
