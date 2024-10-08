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
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using HaselCommon.Extensions.Collections;
using HaselCommon.Extensions.Strings;
using HaselCommon.Game.Enums;
using HaselCommon.Graphics;
using HaselCommon.Gui;
using HaselCommon.Services;
using ImGuiNET;
using Lumina.Data.Files;
using Microsoft.Extensions.Logging;
using MogMogCheck.Caches;
using MogMogCheck.Config;
using MogMogCheck.Records;
using MogMogCheck.Services;
using CharaMakeCustomize = Lumina.Excel.GeneratedSheets.CharaMakeCustomize;
using Companion = Lumina.Excel.GeneratedSheets.Companion;
using Emote = Lumina.Excel.GeneratedSheets.Emote;
using ItemUICategory = Lumina.Excel.GeneratedSheets.ItemUICategory;
using Mount = Lumina.Excel.GeneratedSheets.Mount;
using Ornament = Lumina.Excel.GeneratedSheets.Ornament;
using Picture = Lumina.Excel.GeneratedSheets.Picture;
using TripleTriadCard = Lumina.Excel.GeneratedSheets.TripleTriadCard;
using TripleTriadCardResident = Lumina.Excel.GeneratedSheets.TripleTriadCardResident;

namespace MogMogCheck.Windows;

public unsafe class MainWindow : SimpleWindow
{
    private readonly ILogger<MainWindow> Logger;
    private readonly PluginConfig PluginConfig;
    private readonly IDalamudPluginInterface PluginInterface;
    private readonly ICommandManager CommandManager;
    private readonly IClientState ClientState;
    private readonly IGameInventory GameInventory;
    private readonly ITextureProvider TextureProvider;
    private readonly IDataManager DataManager;
    private readonly SpecialShopService SpecialShopService;
    private readonly TripleTriadNumberFontManager TripleTriadNumberFontManager;
    private readonly ItemService ItemService;
    private readonly ExcelService ExcelService;
    private readonly TextService TextService;
    private readonly TextureService TextureService;
    private readonly ImGuiContextMenuService ImGuiContextMenuService;
    private readonly ItemQuantityCache ItemQuantityCache;
#if DEBUG
    private readonly DebugWindow DebugWindow;
#endif
    private readonly ConfigWindow ConfigWindow;

    private readonly CommandInfo CommandInfo;

    private readonly Dictionary<uint, Vector2?> IconSizeCache = [];

    public MainWindow(
        ILogger<MainWindow> logger,
        PluginConfig pluginConfig,
        WindowManager windowManager,
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IGameInventory gameInventory,
        ITextureProvider textureProvider,
        IDataManager dataManager,
        SpecialShopService specialShopService,
        TripleTriadNumberFontManager tripleTriadNumberFontManager,
        ItemService itemService,
        ExcelService excelService,
        TextService textService,
        TextureService textureService,
        ImGuiContextMenuService imGuiContextMenuService,
        ItemQuantityCache itemQuantityCache,
#if DEBUG
        DebugWindow debugWindow,
#endif
        ConfigWindow configWindow) : base(windowManager, "MogMogCheck")
    {
        Logger = logger;
        PluginConfig = pluginConfig;
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        ClientState = clientState;
        GameInventory = gameInventory;
        TextureProvider = textureProvider;
        DataManager = dataManager;
        SpecialShopService = specialShopService;
        TripleTriadNumberFontManager = tripleTriadNumberFontManager;
        ItemService = itemService;
        ExcelService = excelService;
        TextService = textService;
        TextureService = textureService;
        ImGuiContextMenuService = imGuiContextMenuService;
        ItemQuantityCache = itemQuantityCache;
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
        CommandManager.RemoveHandler("/mogmog");
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
                var cardId = item.ItemAction.Value!.Data[0];
                var cardRow = ExcelService.GetRow<TripleTriadCard>(cardId)!;
                var cardResident = ExcelService.GetRow<TripleTriadCardResident>(cardId)!;
                var cardRarity = cardResident.TripleTriadCardRarity.Value!;

                var cardSize = new Vector2(208, 256);
                var cardSizeScaled = ImGuiHelpers.ScaledVector2(cardSize.X, cardSize.Y);

                using var tooltip = ImRaii.Tooltip();
                ImGui.TextUnformatted($"{(cardResident.TripleTriadCardRarity.Row == 5 ? "Ex" : "No")}. {cardResident.Order} - {cardRow.Name}");
                var pos = ImGui.GetCursorPos();
                TextureService.DrawPart("CardTripleTriad", 1, 0, cardSizeScaled);
                ImGui.SetCursorPos(pos);
                TextureService.DrawIcon(87000 + cardRow.RowId, cardSizeScaled);

                var starSize = cardSizeScaled.Y / 10f;
                var starCenter = pos + new Vector2(starSize);
                var starRadius = starSize / 1.666f;

                static Vector2 GetPosOnCircle(float radius, int index, int numberOfPoints)
                {
                    var angleIncrement = 2 * MathF.PI / numberOfPoints;
                    var angle = index * angleIncrement - MathF.PI / 2;
                    return new Vector2(
                        radius * MathF.Cos(angle),
                        radius * MathF.Sin(angle)
                    );
                }

                if (cardRarity.Stars >= 1)
                {
                    ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 0, 5)); // top
                    TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);

                    if (cardRarity.Stars >= 2)
                    {
                        ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 4, 5)); // left
                        TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);
                    }
                    if (cardRarity.Stars >= 3)
                    {
                        ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 1, 5)); // right
                        TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);
                    }
                    if (cardRarity.Stars >= 4)
                    {
                        ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 3, 5)); // bottom right
                        TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);
                    }
                    if (cardRarity.Stars >= 5)
                    {
                        ImGui.SetCursorPos(starCenter + GetPosOnCircle(starRadius, 2, 5)); // bottom left
                        TextureService.DrawPart("CardTripleTriad", 1, 1, starSize);
                    }
                }

                // type
                if (cardResident.TripleTriadCardType.Row != 0)
                {
                    ImGui.SetCursorPos(pos + new Vector2(cardSize.X, 0) - new Vector2(starSize * 1.5f, -starSize / 2f));
                    TextureService.DrawPart("CardTripleTriad", 1, cardResident.TripleTriadCardType.Row + 2, starSize);
                }

                // numbers
                using var font = TripleTriadNumberFontManager.GetFont().Push();

                var numberText = $"{cardResident.Top:X}";
                var numberTextSize = ImGui.CalcTextSize(numberText);
                var numberTextWidth = numberTextSize.X / 1.333f;
                var numberCenter = pos + new Vector2(cardSizeScaled.X / 2f - numberTextWidth, cardSizeScaled.Y - numberTextSize.Y * 2f);

                static void DrawNumberText(Vector2 numberCenter, float numberTextWidth, int posIndex, string numberText)
                {
                    // shadow
                    ImGui.SetCursorPos(numberCenter + GetPosOnCircle(numberTextWidth, posIndex, 4) + ImGuiHelpers.ScaledVector2(2));
                    using (ImRaii.PushColor(ImGuiCol.Text, 0xFF000000))
                        ImGui.TextUnformatted(numberText);

                    // text
                    ImGui.SetCursorPos(numberCenter + GetPosOnCircle(numberTextWidth, posIndex, 4));
                    ImGui.TextUnformatted(numberText);
                }

                DrawNumberText(numberCenter, numberTextWidth, 0, numberText); // top
                DrawNumberText(numberCenter, numberTextWidth, 1, $"{cardResident.Right:X}"); // right
                DrawNumberText(numberCenter, numberTextWidth, 2, $"{cardResident.Left:X}"); // left
                DrawNumberText(numberCenter, numberTextWidth, 3, $"{cardResident.Bottom:X}"); // bottom
            }
            else if (item.ItemUICategory.Row == 95) // Paintings
            {
                var pictureId = (uint)ExcelService.GetRow<Picture>(item.AdditionalData)!.Image;

                if (!IconSizeCache.TryGetValue(pictureId, out var size))
                {
                    var iconPath = TextureProvider.GetIconPath(pictureId);
                    if (string.IsNullOrEmpty(iconPath))
                    {
                        IconSizeCache.Add(pictureId, null);
                    }
                    else
                    {
                        var file = DataManager.GetFile<TexFile>(iconPath);
                        IconSizeCache.Add(pictureId, size = file != null ? new(file.Header.Width, file.Header.Height) : null);
                    }
                }

                if (size != null)
                {
                    using var tooltip = ImRaii.Tooltip();
                    TextureService.DrawIcon(pictureId, (Vector2)size * 0.5f);
                }
            }
            else if (item.ItemAction.Value?.Type == (uint)ItemActionType.UnlockLink && ExcelService.FindRow<CharaMakeCustomize>(row => row?.HintItem.Row == item.RowId) != null) // Hairstyles etc.
            {
                using var tooltip = ImRaii.Tooltip();

                var tribeId = 1;
                var isMale = false;
                unsafe
                {
                    var character = (Character*)(ClientState.LocalPlayer?.Address ?? 0);
                    if (character != null)
                    {
                        tribeId = character->DrawData.CustomizeData.Tribe;
                        isMale = character->DrawData.CustomizeData.Sex == 0;
                    }
                }

                // TODO: https://discord.com/channels/581875019861328007/653504487352303619/1268886862186152038
                var numHair = 130;
                var startIndex = tribeId switch
                {
                    1 => isMale ? 0 : 1 * numHair, // Midlander
                    2 => isMale ? 2 * numHair : 3 * numHair, // Highlander
                    _ => (tribeId + 2) * numHair
                };

                var charaMakeCustomize = ExcelService.FindRow<CharaMakeCustomize>(row => row?.RowId >= startIndex && row.HintItem.Row == item.RowId);
                if (charaMakeCustomize != null)
                {
                    TextureService.DrawIcon(charaMakeCustomize.Icon, 192);
                }
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
            ImGui.TextUnformatted($"{(quantity > 1 ? quantity.ToString() + "x " : "")}{item.Name}");

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
