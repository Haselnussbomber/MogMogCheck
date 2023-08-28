using System.Linq;
using System.Numerics;
using Dalamud.Interface.Raii;
using Dalamud.Interface.Windowing;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Sheets;
using HaselCommon.Utils;
using ImGuiNET;
using MogMogCheck.Sheets;

namespace MogMogCheck.Windows;

public unsafe class MainWindow : Window
{
    public MainWindow() : base("MogMogCheck")
    {
        Namespace = "MogMogCheckMain";
    }

    public override void OnClose()
    {
        Service.WindowManager.CloseWindow<MainWindow>();
    }

    public override bool DrawConditions()
    {
        return Service.ClientState.IsLoggedIn;
    }

    public override void Draw()
    {
        var shop = GetRow<SpecialShop>(1769929);
        if (shop == null || shop.Items.Length == 0)
            return;

        var scale = ImGui.GetIO().FontGlobalScale;
        
        var tomestone = GetRow<Item>((uint)shop.Items[0].RequiredItem);
        if (tomestone == null)
            return;

        Service.TextureManager.GetIcon(tomestone.Icon).Draw(32 * scale);

        new ImGuiContextMenu($"##Tomestone_ItemContextMenu{tomestone.RowId}_Tooltip")
        {
            ImGuiContextMenu.CreateItemFinder(tomestone.RowId),
            ImGuiContextMenu.CreateCopyItemName(tomestone.RowId),
            ImGuiContextMenu.CreateItemSearch(tomestone.RowId),
            ImGuiContextMenu.CreateOpenOnGarlandTools(tomestone.RowId),
        }
        .Draw();

        ImGui.SameLine(45 * scale);
        ImGuiUtils.PushCursorY(6 * scale);

        var owned = InventoryManager.Instance()->GetInventoryItemCount((uint)shop.Items[0].RequiredItem);
        var needed = shop.Items.Aggregate(0u, (total, item) => total + (Plugin.Config.TrackedItems.TryGetValue(item.ItemId, out var tracked) && tracked ? item.RequiredCount : 0));
        if (needed > owned)
        {
            var remaining = needed - owned;
            ImGui.TextUnformatted(t("Currency.InfoWithRemaining", owned, needed, remaining));
        }
        else
        {
            ImGui.TextUnformatted(t("Currency.Info", owned, needed));
        }

        using var child = ImRaii.Child("##TableWrapper");

        using var table = ImRaii.Table("##Items", 3);

        ImGui.TableSetupColumn(t("TableHeader.Track"), ImGuiTableColumnFlags.WidthFixed, 24);
        ImGui.TableSetupColumn(t("TableHeader.Item"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(t("TableHeader.Price"), ImGuiTableColumnFlags.WidthFixed, 120);

        ImGui.TableHeadersRow();

        for (var i = 0; i < shop.Items.Length; i++)
        {
            var item = shop.Items[i];
            if (item.ItemId == 0)
                continue;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGuiUtils.PushCursorY(4 * scale);

            if (!Plugin.Config.TrackedItems.TryGetValue(item.ItemId, out var tracked))
                Plugin.Config.TrackedItems.Add(item.ItemId, tracked = false);

            if (ImGui.Checkbox($"##Row{i}", ref tracked))
            {
                if (tracked)
                {
                    if (!Plugin.Config.TrackedItems.ContainsKey(item.ItemId))
                        Plugin.Config.TrackedItems.Add(item.ItemId, tracked);
                    else
                        Plugin.Config.TrackedItems[item.ItemId] = tracked;
                }
                else
                {
                    Plugin.Config.TrackedItems.Remove(item.ItemId);
                }

                Plugin.Config.Save();
            }

            ImGui.TableNextColumn();
            DrawItem($"Row{i}", GetRow<Item>((uint)item.ItemId), item.StackSize);

            ImGui.TableNextColumn();
            DrawCurrency($"Row{i}", GetRow<Item>((uint)item.RequiredItem), item.RequiredCount);
        }
    }

    public static void DrawItem(string key, Item? item, uint stackSize)
    {
        if (item == null)
            return;

        var scale = ImGui.GetIO().FontGlobalScale;

        Service.TextureManager.GetIcon(item.Icon).Draw(32 * scale);

        if (item.IsUnlockable && item.HasAcquired)
        {
            ImGui.SameLine(18 * scale, 0);
            ImGuiUtils.PushCursorY(16 * scale);

            var tex = Service.TextureProvider.GetTextureFromGame("ui/uld/RecipeNoteBook_hr1.tex");
            if (tex != null)
            {
                var pos = ImGui.GetWindowPos() + ImGui.GetCursorPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY());
                ImGui.GetWindowDrawList().AddImage(tex.ImGuiHandle, pos + Vector2.Zero, pos + new Vector2(24 * scale), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
            }
        }

        ImGui.SameLine(40 * scale, 0);
        ImGui.Selectable($"##{key}_Item{item.RowId}_Selectable", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 32 * scale));

        new ImGuiContextMenu($"##{key}_ItemContextMenu{item.RowId}_Tooltip")
        {
            ImGuiContextMenu.CreateItemFinder(item.RowId),
            ImGuiContextMenu.CreateCopyItemName(item.RowId),
            ImGuiContextMenu.CreateItemSearch(item.RowId),
            ImGuiContextMenu.CreateOpenOnGarlandTools(item.RowId),
        }
        .Draw();

        var name = $"{(stackSize > 1 ? stackSize.ToString() + "x " : "")}{GetItemName(item.RowId)}";
        ImGui.SameLine(42 * scale, 0);
        ImGuiUtils.PushCursorY(-1 * scale);
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)Colors.GetItemRarityColor(item.Rarity)))
            ImGui.TextUnformatted(name);

        ImGui.SameLine(42 * scale, 0);
        ImGuiUtils.PushCursorY(ImGui.GetFrameHeight() - 9 * scale);
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)Colors.Grey))
            ImGui.TextUnformatted($"{item.ItemUICategory.Value?.Name}");
    }

    public static void DrawCurrency(string key, Item? item, uint needed)
    {
        if (item == null)
            return;

        var scale = ImGui.GetIO().FontGlobalScale;

        Service.TextureManager.GetIcon(item.Icon).Draw(32 * scale);

        new ImGuiContextMenu($"##{key}_ItemContextMenu{item.RowId}_Tooltip")
        {
            ImGuiContextMenu.CreateItemFinder(item.RowId),
            ImGuiContextMenu.CreateCopyItemName(item.RowId),
            ImGuiContextMenu.CreateItemSearch(item.RowId),
            ImGuiContextMenu.CreateOpenOnGarlandTools(item.RowId),
        }
        .Draw();

        ImGui.SameLine(45 * scale);
        ImGuiUtils.PushCursorY(6 * scale);
        ImGui.TextUnformatted($"{needed}");
    }
}
