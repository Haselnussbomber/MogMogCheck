using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.UI.Misc;
using HaselCommon.Extensions;
using HaselCommon.Utils;

namespace MogMogCheck.Caches;

[RegisterSingleton, AutoConstruct]
public partial class ItemQuantityService : IDisposable
{
    private readonly IClientState _clientState;
    private readonly IGameInventory _gameInventory;
    private readonly IFramework _framework;
    private readonly Dictionary<uint, uint> _cache = []; // Key: ItemId, Value: Quantity
    private Debouncer _clearDebouncer;

    public event Action? Cleared;

    [AutoPostConstruct]
    private void Initialize()
    {
        _clearDebouncer = _framework.CreateDebouncer(TimeSpan.FromMilliseconds(500), Clear);
        _clientState.Login += OnLogin;
        _gameInventory.InventoryChangedRaw += OnInventoryChanged;
    }

    public void Dispose()
    {
        _gameInventory.InventoryChangedRaw -= OnInventoryChanged;
        _clientState.Login -= OnLogin;
        _clearDebouncer.Dispose();
    }

    private void OnLogin()
    {
        Clear();
    }

    private void OnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        // Clear cache immediately when an item was added to the Inventory, delay otherwise
        if (events.Any(evt => evt.Type != GameInventoryEvent.Added || (ushort)evt.Item.ContainerType is not (>= 0 and <= 3)))
        {
            _clearDebouncer.Debounce();
        }
        else
        {
            Clear();
        }
    }

    public void Clear()
    {
        _cache.Clear();
        Cleared?.Invoke();
    }

    public unsafe uint Get(ItemHandle item)
    {
        if (_cache.TryGetValue(item, out var result))
            return result;

        var inventoryManager = InventoryManager.Instance();
        var itemFinderModule = ItemFinderModule.Instance();
        var retainerManager = RetainerManager.Instance();

        var count = (uint)inventoryManager->GetInventoryItemCount(item) + (uint)inventoryManager->GetInventoryItemCount(item, true);

        if (itemFinderModule->SaddleBagItemIds.IndexOf(item) is { } saddleBagIndex && saddleBagIndex != -1)
        {
            count += itemFinderModule->SaddleBagItemCount[saddleBagIndex];
        }

        if (itemFinderModule->PremiumSaddleBagItemIds.IndexOf(item) is { } premiumSaddleBagIndex && premiumSaddleBagIndex != -1)
        {
            count += itemFinderModule->PremiumSaddleBagItemCount[premiumSaddleBagIndex];
        }

        if (itemFinderModule->GlamourDresserItemIds.Contains(item))
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
                            for (var i = 0; i < container->GetSize(); i++)
                            {
                                var slot = container->GetInventorySlot(i);
                                if (slot != null && slot->GetItemId() == item)
                                {
                                    count += slot->GetQuantity();
                                }
                            }
                        }
                    }
                }
                else if (retainerInventory.Value->ItemIds.IndexOf(item) is { } retainerIndex && retainerIndex != -1)
                {
                    count += retainerInventory.Value->ItemCount[retainerIndex];
                }
            }
        }

        return _cache[item] = count;
    }
}
