using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.Command;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Interface;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using HaselCommon.Extensions.Collections;
using HaselCommon.Extensions.Strings;
using HaselCommon.Game.Enums;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using MogMogCheck.Caches;
using MogMogCheck.Config;
using MogMogCheck.Records;
using MogMogCheck.Services;
using MogMogCheck.Windows.ItemTooltips;
using CharaMakeCustomize = Lumina.Excel.GeneratedSheets.CharaMakeCustomize;
using Companion = Lumina.Excel.GeneratedSheets.Companion;
using Emote = Lumina.Excel.GeneratedSheets.Emote;
using ItemUICategory = Lumina.Excel.GeneratedSheets.ItemUICategory;
using Mount = Lumina.Excel.GeneratedSheets.Mount;
using Ornament = Lumina.Excel.GeneratedSheets.Ornament;
using Picture = Lumina.Excel.GeneratedSheets.Picture;

namespace MogMogCheck.Windows;

public unsafe class MainWindow : SimpleWindow
{
    private readonly PluginConfig PluginConfig;
    private readonly IDalamudPluginInterface PluginInterface;
    private readonly ICommandManager CommandManager;
    private readonly IClientState ClientState;
    private readonly IGameInventory GameInventory;
    private readonly ITextureProvider TextureProvider;
    private readonly SpecialShopService SpecialShopService;
    private readonly TripleTriadNumberFontManager TripleTriadNumberFontManager;
    private readonly ItemService ItemService;
    private readonly ExcelService ExcelService;
    private readonly TextService TextService;
    private readonly TextureService TextureService;
    private readonly ImGuiContextMenuService ImGuiContextMenuService;
    private readonly ItemQuantityCache ItemQuantityCache;
    private readonly AddonObserver AddonObserver;
#if DEBUG
    private readonly DebugWindow DebugWindow;
#endif
    private readonly ConfigWindow ConfigWindow;

    private readonly CommandInfo CommandInfo;

    private TripleTriadCardTooltip? TripleTriadCardTooltip;

    public MainWindow(
        PluginConfig pluginConfig,
        WindowManager windowManager,
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IGameInventory gameInventory,
        ITextureProvider textureProvider,
        SpecialShopService specialShopService,
        TripleTriadNumberFontManager tripleTriadNumberFontManager,
        ItemService itemService,
        ExcelService excelService,
        TextService textService,
        TextureService textureService,
        ImGuiContextMenuService imGuiContextMenuService,
        ItemQuantityCache itemQuantityCache,
        AddonObserver addonObserver,
#if DEBUG
        DebugWindow debugWindow,
#endif
        ConfigWindow configWindow) : base(windowManager, "MogMogCheck")
    {
        PluginConfig = pluginConfig;
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        GameInventory = gameInventory;
        TextureProvider = textureProvider;
        SpecialShopService = specialShopService;
        TripleTriadNumberFontManager = tripleTriadNumberFontManager;
        ItemService = itemService;
        ExcelService = excelService;
        TextService = textService;
        TextureService = textureService;
        ImGuiContextMenuService = imGuiContextMenuService;
        ItemQuantityCache = itemQuantityCache;
        AddonObserver = addonObserver;
#if DEBUG
        DebugWindow = debugWindow;
#endif
        ConfigWindow = configWindow;

        Size = new Vector2(570, 740);
        SizeCondition = ImGuiCond.FirstUseEver;
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(300, 200),
            MaximumSize = new Vector2(4069),
        };

        Flags |= ImGuiWindowFlags.NoScrollbar;

        TitleBarButtons.Add(new()
        {
            Icon = FontAwesomeIcon.Cog,
            IconOffset = new(0, 1),
            ShowTooltip = () =>
            {
                using var tooltip = ImRaii.Tooltip();
                TextService.Draw($"TitleBarButton.ToggleConfig.Tooltip.{(ConfigWindow.IsOpen ? "Close" : "Open")}Config");
            },
            Click = (button) => ConfigWindow.Toggle()
        });

        PluginInterface.UiBuilder.OpenMainUi += Toggle;
        TextService.LanguageChanged += OnLanguageChanged;
        GameInventory.InventoryChangedRaw += OnInventoryChanged;
        AddonObserver.AddonOpen += AddonObserver_AddonOpen;
        AddonObserver.AddonClose += AddonObserver_AddonClose;

        CommandManager.AddHandler("/mogmog", CommandInfo = new CommandInfo(OnCommand)
        {
            HelpMessage = textService.Translate("CommandHandlerHelpMessage")
        });
    }

    public new void Dispose()
    {
        GameInventory.InventoryChangedRaw -= OnInventoryChanged;
        PluginInterface.UiBuilder.OpenMainUi -= Toggle;
        TextService.LanguageChanged -= OnLanguageChanged;
        AddonObserver.AddonOpen -= AddonObserver_AddonOpen;
        AddonObserver.AddonClose -= AddonObserver_AddonClose;
        CommandManager.RemoveHandler("/mogmog");
        TripleTriadCardTooltip?.Dispose();
        TripleTriadCardTooltip = null;
        base.Dispose();
    }

    private void OnLanguageChanged(string langCode)
    {
        CommandInfo.HelpMessage = TextService.Translate("CommandHandlerHelpMessage");
    }

    private void OnCommand(string command, string arguments)
    {
        switch (arguments.ToLower())
        {
#if DEBUG
            case "debug":
                DebugWindow.Toggle();
                break;
#endif

            case "config":
                ConfigWindow.Toggle();
                break;

            default:
                Toggle();
                break;
        }
    }

    private void AddonObserver_AddonOpen(string addonName)
    {
        if (PluginConfig.OpenWithMogpendium && addonName == "MoogleCollection")
        {
            Open();
        }
    }

    private void AddonObserver_AddonClose(string addonName)
    {
        if (PluginConfig.OpenWithMogpendium && addonName == "MoogleCollection")
        {
            Close();
        }
    }

    private void OnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        ItemQuantityCache.Clear();
    }

    public override void Update()
    {
        if (!SpecialShopService.Update())
            return;

        if (SpecialShopService.CurrentShop == null)
        {
            Close();
            return;
        }

        // clear old untracked items
        if (PluginConfig.TrackedItems.RemoveAll((uint itemId, uint amount) => amount == 0
        || !SpecialShopService.RewardsItems.Any(entry => entry.ReceiveItems.Any(ri => ri.Item?.RowId == itemId))))
        {
            PluginConfig.Save();
        }
    }

    public override bool DrawConditions()
        => ClientState.IsLoggedIn
            && SpecialShopService.CurrentSeason != null
            && SpecialShopService.CurrentShop != null
            && SpecialShopService.RewardsItems != null
            // && RewardsTable?.TotalItems > 0
            && SpecialShopService.TomestoneItems.Count != 0
        ;

    public override void Draw()
    {
        var scale = ImGuiHelpers.GlobalScale;

        foreach (var item in SpecialShopService.TomestoneItems.Values)
        {
            TextureService.DrawIcon(item.Icon, 32 * scale);
            ImGuiContextMenuService.Draw($"##Tomestone_ItemContextMenu{item.RowId}_Tooltip", builder =>
            {
                builder.AddItemFinder(item.RowId);
                builder.AddCopyItemName(item.RowId);
                builder.AddItemSearch(item);
                builder.AddOpenOnGarlandTools("item", item.RowId);
            });

            ImGui.SameLine(45 * scale);
            ImGuiUtils.PushCursorY(6 * scale);

            var needed = 0u;
            for (var i = 0; i < SpecialShopService.CurrentShop!.Item.Length; i++)
            {
                var row = SpecialShopService.CurrentShop!.Item[i];
                if (row.ItemCost[0] != item.RowId && row.ItemCost[1] != item.RowId && row.ItemCost[2] != item.RowId)
                    continue;

                needed += PluginConfig.TrackedItems.TryGetValue(row.Item[0].Row, out var amount) ? amount * row.CurrencyCost[0] : 0u;
            }

            var quantity = ItemQuantityCache.GetValue(item.RowId);
            if (needed > quantity)
            {
                var remaining = needed - quantity;
                TextService.Draw("Currency.InfoWithRemaining", quantity, needed, remaining);
            }
            else
            {
                TextService.Draw("Currency.Info", quantity, needed);
            }
        }

        using var table = ImRaii.Table("RewardsTable", 3, ImGuiTableFlags.ScrollY, new Vector2(-1));
        if (!table) return;

        ImGui.TableSetupColumn(TextService.Translate("Table.Rewards.Header.Track"), ImGuiTableColumnFlags.WidthFixed, (PluginConfig.CheckboxMode ? ImGui.GetFrameHeightWithSpacing() : 80) * ImGuiHelpers.GlobalScale);
        ImGui.TableSetupColumn(TextService.Translate("Table.Rewards.Header.Reward"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(TextService.Translate("Table.Rewards.Header.RequiredItem"), ImGuiTableColumnFlags.WidthFixed, 130);
        ImGui.TableSetupScrollFreeze(3, 1);
        ImGui.TableHeadersRow();

        var idx = 0;
        foreach (var row in SpecialShopService.RewardsItems)
        {
            var item = row.ReceiveItems[0].Item;
            if (item == null || item.RowId == 0)
                continue;

            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            DrawTrackColumn(row, idx);

            ImGui.TableNextColumn();
            DrawRewardColumn(row, idx);

            ImGui.TableNextColumn();
            DrawRequiredItemColumn(row, idx);

            idx++;
        }
    }

    public void DrawTrackColumn(Reward row, int idx)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var itemInnerSpacing = ImGui.GetStyle().ItemInnerSpacing;
        var rowHeight = (ImGui.GetTextLineHeight() + itemInnerSpacing.Y) * 2f;
        var paddingY = (rowHeight - ImGui.GetFrameHeight()) * 0.5f;

        var itemRow = row.ReceiveItems[0].Item!;
        var itemId = itemRow.RowId;

        ImGuiUtils.PushCursor(ImGui.GetStyle().ItemInnerSpacing.X * scale, paddingY);
        ImGui.SetNextItemWidth(-1);

        if (PluginConfig.CheckboxMode)
        {
            if (!PluginConfig.TrackedItems.TryGetValue(itemId, out var savedAmount))
                savedAmount = 0;

            var isChecked = savedAmount > 0;
            if (ImGui.Checkbox($"##Row{idx}", ref isChecked))
            {
                if (isChecked)
                {
                    if (!PluginConfig.TrackedItems.ContainsKey(itemId))
                        PluginConfig.TrackedItems.Add(itemId, 1);
                    else
                        PluginConfig.TrackedItems[itemId] = 1;
                }
                else
                {
                    PluginConfig.TrackedItems.Remove(itemId);
                }

                PluginConfig.Save();
            }

            if (isChecked && (ImGui.IsItemHovered() || ImGui.IsItemActive()))
            {
                ImGui.BeginTooltip();
                TextService.Draw("Reward.AmountInput.Tooltip.ResultOnly", 1 * row.GiveItems[0].Quantity);
                ImGui.EndTooltip();
            }
        }
        else
        {
            var canSell = !itemRow.IsUnique && !itemRow.IsUntradable && !itemRow.IsCollectable;
            var stackSize = canSell ? 999 : itemRow.StackSize;

            if (!PluginConfig.TrackedItems.TryGetValue(itemId, out var savedAmount))
                savedAmount = 0;

            var inputAmount = (int)savedAmount;

            var changed = ImGui.DragInt($"##Row{idx}", ref inputAmount, 1, 0, (int)stackSize, $"%d / {stackSize}", ImGuiSliderFlags.AlwaysClamp);
            if (changed && savedAmount != inputAmount)
            {
                if (inputAmount > 0)
                {
                    if (!PluginConfig.TrackedItems.ContainsKey(itemId))
                        PluginConfig.TrackedItems.Add(itemId, (uint)inputAmount);
                    else
                        PluginConfig.TrackedItems[itemId] = (uint)inputAmount;
                }
                else
                {
                    PluginConfig.TrackedItems.Remove(itemId);
                }

                PluginConfig.Save();
            }

            if (ImGui.IsItemHovered() || ImGui.IsItemActive())
            {
                ImGui.BeginTooltip();
                if (inputAmount <= 1)
                    TextService.Draw("Reward.AmountInput.Tooltip.ResultOnly", inputAmount * row.GiveItems[0].Quantity);
                else
                    TextService.Draw("Reward.AmountInput.Tooltip.Calculation", inputAmount, row.GiveItems[0].Quantity, inputAmount * row.GiveItems[0].Quantity);
                ImGui.EndTooltip();
            }
        }
    }

    private void DrawRewardColumn(Reward row, int idx)
    {
        var scale = ImGuiHelpers.GlobalScale;
        var itemSpacing = ImGui.GetStyle().ItemSpacing;
        var itemInnerSpacing = ImGui.GetStyle().ItemInnerSpacing;

        var textHeight = ImGui.GetTextLineHeight();
        var rowHeight = (textHeight + itemInnerSpacing.Y) * 2f;
        var iconSize = (textHeight + itemInnerSpacing.Y) * 1.5f;
        var textOffsetX = iconSize + itemSpacing.X;

        // TODO: add support for item 2 (see: PLD shields)
        var item = row.ReceiveItems[0].Item!;
        var quantity = row.ReceiveItems[0].Quantity;

        var cursor = ImGui.GetCursorPos();
        ImGuiUtils.PushCursorY((rowHeight - iconSize + itemInnerSpacing.Y) * 0.5f);
        TextureService.DrawIcon(item.Icon, iconSize);

        if (ImGui.IsItemHovered() && !ImGui.IsKeyDown(ImGuiKey.LeftAlt))
        {
            if (item.ItemAction.Value?.Type == (uint)ItemActionType.Mount)
            {
                using var tooltip = ImRaii.Tooltip();
                var mount = ExcelService.GetRow<Mount>(item.ItemAction.Value!.Data[0])!;
                TextureService.DrawIcon(64000 + mount.Icon, 192);
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.Companion)
            {
                using var tooltip = ImRaii.Tooltip();
                var companion = ExcelService.GetRow<Companion>(item.ItemAction.Value!.Data[0])!;
                TextureService.DrawIcon(64000 + companion.Icon, 192);
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.Ornament)
            {
                using var tooltip = ImRaii.Tooltip();
                var ornament = ExcelService.GetRow<Ornament>(item.ItemAction.Value!.Data[0])!;
                TextureService.DrawIcon(59000 + ornament.Icon, 192);
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.UnlockLink && item.ItemAction.Value?.Data[1] == 5211) // Emotes
            {
                using var tooltip = ImRaii.Tooltip();
                var emote = ExcelService.GetRow<Emote>(item.ItemAction.Value!.Data[2])!;
                TextureService.DrawIcon(emote.Icon, 80);
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.TripleTriadCard)
            {
                using var tooltip = ImRaii.Tooltip();
                TripleTriadCardTooltip ??= new TripleTriadCardTooltip(TextureService, ExcelService, TripleTriadNumberFontManager);
                TripleTriadCardTooltip?.SetItem(item);
                TripleTriadCardTooltip?.CalculateLayout();
                TripleTriadCardTooltip?.Update();
                TripleTriadCardTooltip?.Draw();
            }
            else if (item.ItemUICategory.Row == 95) // Paintings
            {
                using var tooltip = ImRaii.Tooltip();
                var pictureId = (uint)ExcelService.GetRow<Picture>(item.AdditionalData)!.Image;
                TextureService.DrawIcon(pictureId, new DrawInfo() { Scale = 0.5f });
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.UnlockLink && ExcelService.FindRow<CharaMakeCustomize>(row => row?.HintItem.Row == item.RowId) != null) // Hairstyles etc.
            {
                using var tooltip = ImRaii.Tooltip();
                TextureService.DrawIcon(ItemService.GetHairstyleIconId(item.RowId), 192);
            }
            else
            {
                using var tooltip = ImRaii.Tooltip();
                TextureService.DrawIcon(item.Icon, 64);
            }
        }

        ImGuiContextMenuService.Draw($"##{idx}_ItemContextMenu{item.RowId}_IconTooltip", builder =>
        {
            builder.AddTryOn(item);
            builder.AddItemFinder(item.RowId);
            builder.AddCopyItemName(item.RowId);
            builder.AddItemSearch(item);
            builder.AddOpenOnGarlandTools("item", item.RowId);
        });

        if (ItemService.IsUnlockable(item) && ItemService.IsUnlocked(item))
        {
            ImGui.SameLine(1, 0);

            if (TextureProvider.GetFromGame("ui/uld/RecipeNoteBook_hr1.tex").TryGetWrap(out var tex, out _))
            {
                var pos = ImGui.GetWindowPos() + ImGui.GetCursorPos() - new Vector2(ImGui.GetScrollX(), ImGui.GetScrollY()) + new Vector2(iconSize / 2.5f + 4 * scale);
                ImGui.GetWindowDrawList().AddImage(tex.ImGuiHandle, pos, pos + new Vector2(iconSize / 1.5f), new Vector2(0.6818182f, 0.21538462f), new Vector2(1, 0.4f));
            }
        }

        ImGui.SameLine(textOffsetX, 0);
        ImGui.SetCursorPosY(cursor.Y);
        ImGui.Selectable($"##{idx}_Item{item.RowId}_Selectable", false, ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, rowHeight - itemSpacing.Y));

        ImGuiContextMenuService.Draw($"##{idx}_ItemContextMenu{item.RowId}_Tooltip", builder =>
        {
            builder.AddTryOn(item);
            builder.AddItemFinder(item.RowId);
            builder.AddCopyItemName(item.RowId);
            builder.AddItemSearch(item);
            builder.AddOpenOnGarlandTools("item", item.RowId);
        });

        ImGui.SameLine(textOffsetX, 0);
        ImGui.SetCursorPosY(cursor.Y);

        ImGuiUtils.PushCursorY(itemInnerSpacing.Y * 0.5f * scale);
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)ItemService.GetItemRarityColor(item.Rarity)))
            ImGui.TextUnformatted($"{(quantity > 1 ? quantity.ToString() + "x " : "")}{ItemService.GetItemName(item)}");

        ImGui.SameLine(textOffsetX, 0);
        ImGui.SetCursorPosY(cursor.Y + textHeight);
        using (ImRaii.PushColor(ImGuiCol.Text, (uint)Color.Grey))
            ImGui.TextUnformatted(ExcelService.GetRow<ItemUICategory>(item.ItemUICategory.Row)!.Name.ExtractText());

        if (row.RequiredQuest != null)
        {
            var isQuestComplete = QuestManager.IsQuestComplete(row.RequiredQuest.RowId);
            var regionWidth = ImGui.GetContentRegionAvail().X;
            ImGui.SameLine(regionWidth - ImGuiUtils.GetIconSize(FontAwesomeIcon.InfoCircle).X - itemSpacing.X);
            ImGuiUtils.PushCursorY(itemInnerSpacing.Y * 0.5f * scale);
            TextureService.DrawIcon(row.RequiredQuest.EventIconType.Value!.MapIconAvailable + (isQuestComplete ? 5u : 1u), ImGui.GetFrameHeight());

            if (ImGui.IsItemHovered())
            {
                using (ImRaii.Tooltip())
                {
                    var status = isQuestComplete
                        ? TextService.Translate("Reward.RequiredQuest.Tooltip.Complete")
                        : TextService.Translate("Reward.RequiredQuest.Tooltip.Incomplete");
                    TextService.Draw("Reward.RequiredQuest.Tooltip", TextService.GetQuestName(row.RequiredQuest.RowId), status);
                }
            }

            ImGuiContextMenuService.Draw($"##{idx}_ItemContextMenu{item.RowId}_RequiredQuest{row.RequiredQuest.RowId}", builder =>
            {
                builder.AddOpenOnGarlandTools("quest", row.RequiredQuest.RowId);
            });
        }
    }

    private void DrawRequiredItemColumn(Reward row, int idx)
    {
        var itemSpacing = ImGui.GetStyle().ItemSpacing;
        var itemInnerSpacing = ImGui.GetStyle().ItemInnerSpacing;
        var textHeight = ImGui.GetTextLineHeight();
        var rowHeight = (textHeight + itemInnerSpacing.Y) * 2f;
        var iconSize = (textHeight + itemInnerSpacing.Y) * 1.5f;
        var paddingY = (rowHeight - iconSize) * 0.5f;

        // TODO: add support for items 2 and 3 whenever it becomes necessary
        var (item, quantity) = row.GiveItems[0];
        if (item == null)
            return;

        var hasEnoughTomestones =
            SpecialShopService.TomestoneItems.ContainsKey(item.RowId)
            && ItemQuantityCache.GetValue(item.RowId) >= quantity;

        ImGuiUtils.PushCursorY(paddingY);
        TextureService.DrawIcon(item.Icon, new DrawInfo(iconSize) { TintColor = hasEnoughTomestones ? null : (Vector4)Color.Grey });

        ImGuiContextMenuService.Draw($"##{row.Index}_ItemContextMenu{item.RowId}_Tooltip", builder =>
        {
            builder.AddItemFinder(item.RowId);
            builder.AddCopyItemName(item.RowId);
            builder.AddItemSearch(item);
            builder.AddOpenOnGarlandTools("item", item.RowId);
        });

        ImGui.SameLine(iconSize + itemSpacing.X);
        ImGuiUtils.PushCursorY(paddingY);

        using (ImRaii.Disabled(!hasEnoughTomestones))
            ImGui.TextUnformatted(quantity.ToString());
    }
}
