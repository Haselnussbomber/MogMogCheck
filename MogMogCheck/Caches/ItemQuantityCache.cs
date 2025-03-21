using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Cache;

namespace MogMogCheck.Caches;

[RegisterSingleton, AutoConstruct]
public partial class ItemQuantityCache : MemoryCache<uint, uint>
{
    private readonly IClientState _clientState;
    private readonly IGameInventory _gameInventory;
    private readonly IFramework _framework;
    private CancellationTokenSource? _cancellationTokenSource;

    [AutoPostConstruct]
    private void Initialize()
    {
        _clientState.Login -= ClientState_Login;
        _gameInventory.InventoryChangedRaw += GameInventory_InventoryChangedRaw;
    }

    public override void Dispose()
    {
        _clientState.Login -= ClientState_Login;
        _gameInventory.InventoryChangedRaw -= GameInventory_InventoryChangedRaw;
        _cancellationTokenSource?.Cancel();
        base.Dispose();
    }

    private void ClientState_Login()
    {
        Clear();
    }

    private void GameInventory_InventoryChangedRaw(IReadOnlyCollection<InventoryEventArgs> events)
    {
        // Clear cache immediately when an item was added to the Inventory, delay otherwise
        if (events.Any(evt => evt.Type != GameInventoryEvent.Added || (ushort)evt.Item.ContainerType is not (>= 0 and <= 3)))
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            _framework.RunOnTick(Clear, TimeSpan.FromMilliseconds(500), cancellationToken: _cancellationTokenSource.Token);
        }
        else
        {
            Clear();
        }
    }

    public override unsafe uint CreateEntry(uint itemId)
    {
        var inventoryManager = InventoryManager.Instance();
        var itemFinderModule = ItemFinderModule.Instance();
        var retainerManager = RetainerManager.Instance();

        var count = (uint)inventoryManager->GetInventoryItemCount(itemId) + (uint)inventoryManager->GetInventoryItemCount(itemId, true);

        if (itemFinderModule->SaddleBagItemIds.IndexOf(itemId) is { } saddleBagIndex && saddleBagIndex != -1)
        {
            count += itemFinderModule->SaddleBagItemCount[saddleBagIndex];
        }

        if (itemFinderModule->PremiumSaddleBagItemIds.IndexOf(itemId) is { } premiumSaddleBagIndex && premiumSaddleBagIndex != -1)
        {
            count += itemFinderModule->PremiumSaddleBagItemCount[premiumSaddleBagIndex];
        }

        if (itemFinderModule->GlamourDresserItemIds.Contains(itemId))
        {
            count += 1;
        }

        if (retainerManager->IsReady)
        {
            foreach (var (_, retainerInventory) in itemFinderModule->RetainerInventories)
            {
                if (retainerManager->RetainerObjectId != 0xE0000000) // If retainer is active
                {
                    foreach (var type in Enumerable.Range((int)InventoryType.RetainerPage1, 7))
                    {
                        var container = inventoryManager->GetInventoryContainer((InventoryType)type);
                        if (container != null && container->IsLoaded)
                        {
                            foreach (ref var item in new Span<InventoryItem>(container->Items, (int)container->Size))
                            {
                                if (item.GetItemId() == itemId)
                                {
                                    count += item.GetQuantity();
                                }
                            }
                        }
                    }
                }
                else if (retainerInventory.Value->ItemIds.IndexOf(itemId) is { } retainerIndex && retainerIndex != -1)
                {
                    count += retainerInventory.Value->ItemCount[retainerIndex];
                }
            }
        }

        return count;
    }
}
